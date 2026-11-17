using System.Data;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Domain;

namespace RetailPOS.Infrastructure;

public sealed partial class Operaciones(TiendaDb db) : IOperaciones
{
    private Task BloquearCajaAsync(string usuarioId, CancellationToken ct)
    {
        var recurso = "CajaUsuario:" + usuarioId;
        return db.Database.ExecuteSqlAsync($"DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource={recurso}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000; IF @r<0 THROW 51000, 'No se pudo bloquear la caja.',1;", ct);
    }
    private static void ValidarImporte(decimal importe)
    {
        if (importe < 0 || importe > 999999999 || decimal.Truncate(importe) != importe)
            throw new ReglaNegocioException("Ingresa un importe entero en pesos, entre 0 y 999.999.999.");
    }

    public async Task MoverStockAsync(int varianteId, int bodegaId, int cantidad, string motivo, string usuarioId, CancellationToken ct = default)
    {
        if (cantidad == 0 || cantidad is < -100000 or > 100000 || string.IsNullOrWhiteSpace(motivo) || motivo.Length > 200)
            throw new ReglaNegocioException("Indica una cantidad distinta de cero y un motivo de hasta 200 caracteres.");
        await using var tx = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        if (!await db.Variantes.AnyAsync(x => x.Id == varianteId && x.Activo && x.Producto.Activo, ct) || !await db.Bodegas.AnyAsync(x => x.Id == bodegaId, ct))
            throw new ReglaNegocioException("Selecciona una variante activa y una bodega existente.");
        var stock = await db.Existencias.SingleOrDefaultAsync(x => x.VarianteId == varianteId && x.BodegaId == bodegaId, ct);
        if (stock is null)
        {
            stock = new Existencia { VarianteId = varianteId, BodegaId = bodegaId };
            db.Add(stock);
        }
        if (stock.Cantidad + cantidad < 0) throw new ReglaNegocioException("El ajuste dejaría existencias negativas.");
        stock.Cantidad += cantidad;
        db.Add(new Movimiento { VarianteId = varianteId, BodegaId = bodegaId, Cantidad = cantidad, Motivo = motivo.Trim(), UsuarioId = usuarioId });
        await db.SaveChangesAsync(ct);
        if (tx is not null) await tx.CommitAsync(ct);
    }

    public Task<int> VenderAsync(SolicitudVenta s, string usuarioId, CancellationToken ct = default) => VenderCoreAsync(s, usuarioId, 0, ct);

    private async Task<int> VenderCoreAsync(SolicitudVenta s, string usuarioId, decimal creditoCambio, CancellationToken ct)
    {
        if (s.SolicitudId == Guid.Empty || s.Lineas.Count is < 1 or > 100 || s.Lineas.Any(x => x.Cantidad is < 1 or > 10000) || s.TipoDocumento is not (33 or 39) || s.Pagos.Count > 4 || (s.Pagos.Count == 0 && s.MedioPago is not ("Efectivo" or "Tarjeta" or "Transferencia" or "Otros")))
            throw new ReglaNegocioException("Revisa las líneas, el documento y el medio de pago.");
        await using var tx = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        // SQL application lock serializa los reintentos de la misma solicitud entre procesos.
        await BloquearCajaAsync(usuarioId, ct);
        var recurso = "Venta:" + s.SolicitudId;
        await db.Database.ExecuteSqlAsync($"DECLARE @r int; EXEC @r = sys.sp_getapplock @Resource={recurso}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000; IF @r < 0 THROW 51000, 'No se pudo bloquear la solicitud.', 1;", ct);
        var existente = await db.Ventas.SingleOrDefaultAsync(x => x.SolicitudId == s.SolicitudId, ct);
        if (existente is not null)
        {
            if (existente.UsuarioId != usuarioId) throw new ReglaNegocioException("La solicitud pertenece a otro usuario.");
            return existente.Id;
        }
        var caja = await db.SesionesCaja.SingleOrDefaultAsync(x => x.UsuarioId == usuarioId && x.Cierre == null, ct)
            ?? throw new ReglaNegocioException("Abre tu caja antes de vender.");
        ValidarDiaCaja(caja);
        var cliente = s.ClienteId is null ? null : await db.Clientes.FindAsync([s.ClienteId.Value], ct);
        if (s.ClienteId is not null && cliente is null) throw new ReglaNegocioException("El cliente no existe.");
        if (s.TipoDocumento == 33 && (cliente is null || string.IsNullOrWhiteSpace(cliente.Rut) || string.IsNullOrWhiteSpace(cliente.Giro) || string.IsNullOrWhiteSpace(cliente.Direccion) || string.IsNullOrWhiteSpace(cliente.Comuna)))
            throw new ReglaNegocioException("Para factura completa RUT, giro, dirección y comuna del cliente.");
        if (!await db.Bodegas.AnyAsync(x => x.Id == s.BodegaId, ct)) throw new ReglaNegocioException("La bodega no existe.");
        var venta = new Venta { SolicitudId = s.SolicitudId, UsuarioId = usuarioId, ClienteId = s.ClienteId, BodegaId = s.BodegaId, SesionCajaId = caja.Id, MedioPago = s.MedioPago };
        venta.NombreCliente = cliente?.Nombre ?? "Consumidor final"; venta.RutCliente=cliente?.Rut; venta.GiroCliente=cliente?.Giro; venta.DireccionCliente=cliente?.Direccion; venta.ComunaCliente=cliente?.Comuna;
        var ofertas = await new Precios(db).VigentesAsync(ct);
        var fechaPrecio = Precios.HoyChile;
        foreach (var grupo in s.Lineas.GroupBy(x => x.VarianteId).OrderBy(x => x.Key))
        {
            var cantidad = grupo.Sum(x => x.Cantidad);
            var variante = await db.Variantes.Include(x => x.Producto).SingleOrDefaultAsync(x => x.Id == grupo.Key && x.Activo && x.Producto.Activo, ct)
                ?? throw new ReglaNegocioException("Una variante no está disponible.");
            var stock = await db.Existencias.SingleOrDefaultAsync(x => x.VarianteId == variante.Id && x.BodegaId == s.BodegaId, ct);
            if (!await db.Marcas.AnyAsync(x => x.Nombre == variante.Producto.Marca && x.Activa, ct) || !await db.Tallas.AnyAsync(x => x.Valor == variante.Talla && x.Sistema == variante.SistemaTalla && x.Activa, ct)) throw new ReglaNegocioException("La marca o talla de esta variante está inactiva.");
            if (stock is null || stock.Cantidad < cantidad) throw new ReglaNegocioException($"Stock insuficiente para {variante.Sku}.");
            var costoVenta = stock.CostoPromedio;
            stock.Cantidad -= cantidad;
            var precio = MotorOfertas.Calcular(variante, ofertas, fechaPrecio);
            venta.Detalles.Add(new DetalleVenta { VarianteId = variante.Id, Cantidad = cantidad, CostoUnitario = costoVenta, MarcaVenta = variante.Producto.Marca, PrecioUnitario = precio.Final, PrecioLista = precio.Base, DescuentoUnitario = precio.Descuento, OfertaId = precio.OfertaId, OfertaNombre = precio.OfertaNombre, Descripcion = $"{variante.Producto.Marca} {variante.Producto.Modelo} · {variante.Color} · {variante.SistemaTalla} {variante.Talla}" });
        }
        venta.Total = venta.Detalles.Sum(x => x.Cantidad * x.PrecioUnitario);
        var pagos = s.Pagos.Count == 0 ? [new PagoSolicitud { Medio = s.MedioPago, Importe = venta.Total }] : s.Pagos;
        if (pagos.Any(x => x.Importe <= 0 || decimal.Truncate(x.Importe) != x.Importe || (x.Medio is not ("Efectivo" or "Tarjeta" or "Transferencia" or "Otros") && !(x.Medio == "SaldoCambio" && creditoCambio > 0))) || pagos.Sum(x => x.Importe) != venta.Total || pagos.Where(x => x.Medio == "SaldoCambio").Sum(x => x.Importe) > creditoCambio)
            throw new ReglaNegocioException("Los pagos deben ser enteros positivos y sumar exactamente el total de la venta.");
        venta.MedioPago = pagos.Select(x => x.Medio).Distinct().Count() == 1 ? pagos[0].Medio : "Mixto";
        venta.Pagos = pagos.Select(x => new PagoVenta { SesionCajaId = caja.Id, Medio = x.Medio, Importe = x.Importe }).ToList();
        db.Add(venta);
        await db.SaveChangesAsync(ct);
        foreach (var linea in venta.Detalles)
            db.Add(new Movimiento { VarianteId = linea.VarianteId, BodegaId = venta.BodegaId, Cantidad = -linea.Cantidad, Motivo = "Venta", UsuarioId = usuarioId, VentaId = venta.Id });
        db.Add(new Documento { VentaId = venta.Id, Tipo = s.TipoDocumento, Total = venta.Total, UsuarioId = usuarioId, Motivo = "Venta presencial / demostración", Detalles = CopiarLineas(venta) });
        await db.SaveChangesAsync(ct);
        if (tx is not null) await tx.CommitAsync(ct);
        return venta.Id;
    }

    public async Task AbrirCajaAsync(decimal fondo, string usuarioId, CancellationToken ct = default)
    {
        ValidarImporte(fondo);
        await using var tx = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        await BloquearCajaAsync(usuarioId, ct);
        if (await db.SesionesCaja.AnyAsync(x => x.UsuarioId == usuarioId && x.Cierre == null, ct)) throw new ReglaNegocioException("Ya tienes una caja abierta.");
        db.Add(new SesionCaja { UsuarioId = usuarioId, FondoInicial = fondo,FechaOperativa=Precios.HoyChile });
        await db.SaveChangesAsync(ct);
        if (tx is not null) await tx.CommitAsync(ct);
    }

    public Task CerrarCajaAsync(decimal contado, string usuarioId, CancellationToken ct = default)
        => CerrarCajaDetalladaAsync(new() { Contado = contado, Tarjeta = 0, Transferencia = 0 }, usuarioId, ct);

    public async Task CerrarCajaDetalladaAsync(SolicitudCuadratura s, string usuarioId, CancellationToken ct = default)
    {
        if (s.Contado is null || s.Tarjeta is null || s.Transferencia is null||s.Otros is null) throw new ReglaNegocioException("Declara efectivo, tarjetas, transferencias y otros medios, aunque sean cero.");
        ValidarImporte(s.Contado.Value);
        if(Math.Abs(s.Otros.Value)>999999999||decimal.Truncate(s.Otros.Value)!=s.Otros.Value)throw new ReglaNegocioException("Declara otros medios en pesos enteros.");
        if (Math.Abs(s.Tarjeta.Value) > 999999999 || Math.Abs(s.Transferencia.Value) > 999999999 || decimal.Truncate(s.Tarjeta.Value) != s.Tarjeta.Value || decimal.Truncate(s.Transferencia.Value) != s.Transferencia.Value) throw new ReglaNegocioException("Declara importes enteros en pesos; tarjeta y transferencia pueden ser negativos por reembolsos.");
        if (s.Observacion?.Length > 500) throw new ReglaNegocioException("La observación admite hasta 500 caracteres.");
        await using var tx = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        await BloquearCajaAsync(usuarioId, ct);
        var caja = await db.SesionesCaja.SingleOrDefaultAsync(x => x.UsuarioId == usuarioId && x.Cierre == null, ct)
            ?? throw new ReglaNegocioException("No tienes una caja abierta.");
        var filas = await CuadraturaAsync(caja, ct);
        var resumen=await ResumenCajaAsync(caja,ct);
        if(resumen.DiferenciaRegistro!=0)throw new ReglaNegocioException("El total documentado y los pagos registrados no coinciden. Revisa las operaciones antes de cerrar.");
        foreach (var fila in filas) fila.Contado = fila.Medio switch { "Efectivo" => s.Contado.Value, "Tarjeta" => s.Tarjeta.Value,"Transferencia"=>s.Transferencia.Value, _ => s.Otros.Value };
        if (filas.Any(x => x.Esperado != x.Contado) && string.IsNullOrWhiteSpace(s.Observacion)) throw new ReglaNegocioException("Explica la diferencia antes de cerrar una caja descuadrada.");
        caja.Cuadraturas = filas;
        caja.EfectivoEsperado = filas.Single(x => x.Medio == "Efectivo").Esperado;
        caja.EfectivoContado = s.Contado.Value;
        caja.ObservacionCierre = s.Observacion?.Trim();
        caja.Cierre = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        if (tx is not null) await tx.CommitAsync(ct);
    }

    public async Task<List<CuadraturaMedio>> CuadraturaAsync(SesionCaja caja, CancellationToken ct = default)
    {
        var filas = new List<CuadraturaMedio>();
        foreach (var medio in MediosCaja)
        {
            var ventas = await db.Pagos.Where(x => x.SesionCajaId == caja.Id && x.Medio == medio).SumAsync(x => (decimal?)x.Importe, ct) ?? 0;
            var reembolsos = await db.Reembolsos.Where(x => x.SesionCajaId == caja.Id && x.Medio == medio).SumAsync(x => (decimal?)x.Importe, ct) ?? 0;
            var ingresos = medio != "Efectivo" ? 0 : await db.MovimientosCaja.Where(x => x.SesionCajaId == caja.Id && x.Tipo == "Ingreso").SumAsync(x => (decimal?)x.Importe, ct) ?? 0;
            var retiros = medio != "Efectivo" ? 0 : await db.MovimientosCaja.Where(x => x.SesionCajaId == caja.Id && x.Tipo == "Retiro").SumAsync(x => (decimal?)x.Importe, ct) ?? 0;
            var fondo = medio == "Efectivo" ? caja.FondoInicial : 0;
            filas.Add(new() { SesionCajaId = caja.Id, Medio = medio, Ventas = ventas, Reembolsos = reembolsos, Fondo = fondo, Ingresos = ingresos, Retiros = retiros, Esperado = fondo + ventas - reembolsos + ingresos - retiros });
        }
        return filas;
    }

    public async Task MovimientoCajaAsync(Guid solicitudId, string tipo, decimal importe, string motivo, string usuarioId, CancellationToken ct = default)
    {
        ValidarImporte(importe);
        if (solicitudId == Guid.Empty || importe == 0 || tipo is not ("Ingreso" or "Retiro") || string.IsNullOrWhiteSpace(motivo) || motivo.Length > 250) throw new ReglaNegocioException("Indica tipo, importe positivo y motivo del movimiento.");
        await using var tx = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        await BloquearCajaAsync(usuarioId, ct);
        var anterior = await db.MovimientosCaja.SingleOrDefaultAsync(x => x.SolicitudId == solicitudId, ct);
        if (anterior is not null)
        {
            if (anterior.UsuarioId != usuarioId) throw new ReglaNegocioException("La solicitud pertenece a otro usuario.");
            return;
        }
        var caja = await db.SesionesCaja.SingleOrDefaultAsync(x => x.UsuarioId == usuarioId && x.Cierre == null, ct) ?? throw new ReglaNegocioException("Abre tu caja antes de registrar movimientos.");
        ValidarDiaCaja(caja);
        if (tipo == "Retiro")
        {
            var filas = await CuadraturaAsync(caja, ct);
            if (importe > filas.Single(x => x.Medio == "Efectivo").Esperado) throw new ReglaNegocioException("El retiro supera el efectivo disponible registrado.");
        }
        db.Add(new MovimientoCaja { SolicitudId = solicitudId, SesionCajaId = caja.Id, Tipo = tipo, Importe = importe, Motivo = motivo.Trim(), UsuarioId = usuarioId });
        await db.SaveChangesAsync(ct);
        if (tx is not null) await tx.CommitAsync(ct);
    }

    public async Task<int> CrearDocumentoAsync(int ventaId, int tipo, int? referenciaId, string motivo, string usuarioId, CancellationToken ct = default)
    {
        if (tipo != 52) throw new ReglaNegocioException("Esta etapa permite agregar una guía simulada a una venta; las notas se implementarán con devoluciones y ajustes.");
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Length > 250) throw new ReglaNegocioException("Indica el motivo (hasta 250 caracteres).");
        var venta = await db.Ventas.Include(x => x.Detalles).SingleOrDefaultAsync(x => x.Id == ventaId, ct) ?? throw new ReglaNegocioException("La venta no existe.");
        var referencia = await db.Documentos.SingleOrDefaultAsync(x => x.Id == referenciaId && x.VentaId == ventaId && (x.Tipo == 33 || x.Tipo == 39), ct)
            ?? throw new ReglaNegocioException("Selecciona el documento de venta como referencia.");
        if (await db.Documentos.AnyAsync(x => x.VentaId == ventaId && x.Tipo == 52, ct)) throw new ReglaNegocioException("La venta ya tiene una guía simulada.");
        var doc = new Documento { VentaId = ventaId, Tipo = tipo, ReferenciaId = referencia.Id, Total = venta.Total, UsuarioId = usuarioId, Motivo = motivo.Trim(), Detalles = CopiarLineas(venta) };
        db.Add(doc);
        await db.SaveChangesAsync(ct);
        return doc.Id;
    }

    private static List<DetalleDocumento> CopiarLineas(Venta venta) => venta.Detalles.Select(x => new DetalleDocumento { Descripcion = x.Descripcion, Cantidad = x.Cantidad, PrecioUnitario = x.PrecioUnitario }).ToList();

    public Task<int> DevolverAsync(Guid solicitudId, int detalleVentaId, int cantidad, bool reingresarStock, string motivo, string usuarioId, CancellationToken ct = default) => DevolverCoreAsync(solicitudId, detalleVentaId, cantidad, reingresarStock, motivo, usuarioId, false, ct);

    private async Task<int> DevolverCoreAsync(Guid solicitudId, int detalleVentaId, int cantidad, bool reingresarStock, string motivo, string usuarioId, bool cambio, CancellationToken ct)
    {
        if (solicitudId == Guid.Empty || cantidad is < 1 or > 10000 || string.IsNullOrWhiteSpace(motivo) || motivo.Length > 250) throw new ReglaNegocioException("Indica cantidad y motivo válidos.");
        await using var tx = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        await BloquearCajaAsync(usuarioId, ct);
        var recurso = "Devolucion:" + detalleVentaId;
        await db.Database.ExecuteSqlAsync($"DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource={recurso}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000; IF @r<0 THROW 51000, 'No se obtuvo bloqueo.',1;", ct);
        var anterior = await db.Devoluciones.SingleOrDefaultAsync(x => x.SolicitudId == solicitudId, ct);
        if (anterior is not null)
        {
            if (anterior.UsuarioId != usuarioId) throw new ReglaNegocioException("La solicitud pertenece a otro usuario.");
            return anterior.DocumentoId;
        }
        var linea = await db.DetallesVenta.Include(x => x.Venta).SingleOrDefaultAsync(x => x.Id == detalleVentaId, ct) ?? throw new ReglaNegocioException("La línea de venta no existe.");
        var devuelto = await db.Devoluciones.Where(x => x.DetalleVentaId == detalleVentaId).SumAsync(x => (int?)x.Cantidad, ct) ?? 0;
        if (cantidad > linea.Cantidad - devuelto) throw new ReglaNegocioException("No puedes devolver más unidades que las pendientes de devolución.");
        var caja = await db.SesionesCaja.SingleOrDefaultAsync(x => x.UsuarioId == usuarioId && x.Cierre == null, ct) ?? throw new ReglaNegocioException("Abre tu caja para registrar el reembolso.");
        ValidarDiaCaja(caja);
        var referencia = await db.Documentos.SingleAsync(x => x.VentaId == linea.VentaId && (x.Tipo == 33 || x.Tipo == 39), ct);
        var importe = cantidad * linea.PrecioUnitario;
        var documento = new Documento { VentaId = linea.VentaId, Tipo = 61, ReferenciaId = referencia.Id, Motivo = motivo.Trim(), Total = importe, UsuarioId = usuarioId,
            Detalles = [new DetalleDocumento { Descripcion = linea.Descripcion, Cantidad = cantidad, PrecioUnitario = linea.PrecioUnitario }] };
        db.Add(documento);
        var devolucion = new Devolucion { SolicitudId = solicitudId, DetalleVentaId = linea.Id, Cantidad = cantidad, ReingresarStock = reingresarStock, Documento = documento, SesionCajaId = caja.Id, Reembolso = importe, MedioReembolso = cambio ? "SaldoCambio" : linea.Venta.MedioPago, UsuarioId = usuarioId };
        db.Add(devolucion);
        if (!cambio)
        {
            var pagosOriginales = await db.Pagos.Where(x => x.VentaId == linea.VentaId && x.DocumentoId == null).GroupBy(x => x.Medio).Select(g => new { Medio = g.Key, Importe = g.Sum(x => x.Importe) }).OrderBy(x => x.Medio).ToListAsync(ct);
            var pendiente = importe;
            foreach (var pago in pagosOriginales)
            {
                var medio = pago.Medio == "SaldoCambio" ? "Efectivo" : pago.Medio;
                var yaDevuelto = await db.Reembolsos.Where(x => x.Devolucion.DetalleVenta.VentaId == linea.VentaId && x.Medio == medio).SumAsync(x => (decimal?)x.Importe, ct) ?? 0;
                var monto = Math.Min(pendiente, Math.Max(0, pago.Importe - yaDevuelto));
                if (monto > 0) db.Add(new ReembolsoPago { Devolucion = devolucion, SesionCajaId = caja.Id, Medio = medio, Importe = monto });
                pendiente -= monto;
            }
            if (pendiente != 0) throw new ReglaNegocioException("No hay saldo de pago suficiente para el reembolso.");
            var efectivoReembolsar=db.ChangeTracker.Entries<ReembolsoPago>().Where(x=>x.State==EntityState.Added&&ReferenceEquals(x.Entity.Devolucion,devolucion)&&x.Entity.Medio=="Efectivo").Sum(x=>x.Entity.Importe);
            if(efectivoReembolsar>0&&(await CuadraturaAsync(caja,ct)).Single(x=>x.Medio=="Efectivo").Esperado<efectivoReembolsar)throw new ReglaNegocioException("El reembolso supera el efectivo disponible. Registra primero un ingreso de caja.");
        }
        if (reingresarStock)
        {
            var stock = await db.Existencias.SingleAsync(x => x.VarianteId == linea.VarianteId && x.BodegaId == linea.Venta.BodegaId, ct);
            stock.CostoPromedio = Promedio(stock.Cantidad, stock.CostoPromedio, cantidad, linea.CostoUnitario);
            stock.Cantidad += cantidad;
            db.Add(new Movimiento { VarianteId = linea.VarianteId, BodegaId = linea.Venta.BodegaId, Cantidad = cantidad, Motivo = "Devolución: " + motivo.Trim()[..Math.Min(200, motivo.Trim().Length)], VentaId = linea.VentaId, UsuarioId = usuarioId });
        }
        else db.Add(new RevisionStock { VarianteId = linea.VarianteId, BodegaId = linea.Venta.BodegaId, Devolucion = devolucion, Cantidad = cantidad, CostoUnitario=linea.CostoUnitario, Motivo = motivo.Trim(), UsuarioId = usuarioId });
        await db.SaveChangesAsync(ct);
        if (tx is not null) await tx.CommitAsync(ct);
        return documento.Id;
    }

    public async Task<int> DebitarAsync(Guid solicitudId, int referenciaId, decimal importe, string motivo, string usuarioId, CancellationToken ct = default)
    {
        ValidarImporte(importe);
        if (solicitudId == Guid.Empty || importe == 0 || string.IsNullOrWhiteSpace(motivo) || motivo.Length > 250) throw new ReglaNegocioException("Indica un importe positivo y un motivo.");
        await using var tx = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        var recurso = "Debito:" + solicitudId;
        await db.Database.ExecuteSqlAsync($"DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource={recurso}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000; IF @r<0 THROW 51000, 'No se obtuvo bloqueo.',1;", ct);
        var anterior = await db.Documentos.SingleOrDefaultAsync(x => x.SolicitudId == solicitudId, ct);
        if (anterior is not null)
        {
            if (anterior.UsuarioId != usuarioId) throw new ReglaNegocioException("La solicitud pertenece a otro usuario.");
            return anterior.Id;
        }
        var referencia = await db.Documentos.SingleOrDefaultAsync(x => x.Id == referenciaId && (x.Tipo == 33 || x.Tipo == 39), ct) ?? throw new ReglaNegocioException("La referencia debe ser una boleta o factura simulada.");
        var doc = new Documento { SolicitudId = solicitudId, VentaId = referencia.VentaId, Tipo = 56, ReferenciaId = referencia.Id, Total = importe, Motivo = motivo.Trim(), UsuarioId = usuarioId,
            Detalles = [new DetalleDocumento { Descripcion = motivo.Trim(), Cantidad = 1, PrecioUnitario = importe }] };
        db.Add(doc);
        await db.SaveChangesAsync(ct);
        if (tx is not null) await tx.CommitAsync(ct);
        return doc.Id;
    }
}



