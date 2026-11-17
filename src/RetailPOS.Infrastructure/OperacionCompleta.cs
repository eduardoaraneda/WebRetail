using System.Data;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Domain;
namespace RetailPOS.Infrastructure;

public sealed partial class Operaciones
{
    private static decimal? Promedio(int unidades, decimal? costo, int ingreso, decimal? nuevoCosto)
        => unidades == 0 ? nuevoCosto : costo is null || nuevoCosto is null ? null : decimal.Round((unidades * costo.Value + ingreso * nuevoCosto.Value) / (unidades + ingreso), 2);

    private Task BloquearAsync(string recurso, CancellationToken ct) => db.Database.ExecuteSqlAsync($"DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource={recurso}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000; IF @r<0 THROW 51000,'Operación concurrente. Reintenta.',1;", ct);
    private async Task<Existencia> StockAsync(int varianteId, int bodegaId, CancellationToken ct)
    {
        if (!await db.Variantes.AnyAsync(x => x.Id == varianteId, ct) || !await db.Bodegas.AnyAsync(x => x.Id == bodegaId, ct)) throw new ReglaNegocioException("Variante o bodega inexistente.");
        var stock = await db.Existencias.SingleOrDefaultAsync(x => x.VarianteId == varianteId && x.BodegaId == bodegaId, ct);
        if (stock is null) { stock = new() { VarianteId = varianteId, BodegaId = bodegaId }; db.Add(stock); }
        return stock;
    }
    private void Movimiento(Existencia stock, int cantidad, string motivo, string usuario) => db.Add(new Movimiento { VarianteId = stock.VarianteId, BodegaId = stock.BodegaId, Cantidad = cantidad, Motivo = motivo, UsuarioId = usuario });

    public async Task<int> ComprarAsync(SolicitudCompra s, string usuario, CancellationToken ct = default)
    {
        if (s.SolicitudId == Guid.Empty || string.IsNullOrWhiteSpace(s.Referencia) || s.Referencia.Length > 80 || s.Lineas.Count is < 1 or > 100 || s.Lineas.GroupBy(x=>x.VarianteId).Any(g=>g.Count()>1) || s.Lineas.Any(x => x.Cantidad is < 1 or > 100000 || x.CostoUnitario is < 0 or > 999999999 || decimal.Round(x.CostoUnitario, 2) != x.CostoUnitario)) throw new ReglaNegocioException("Revisa referencia, cantidades y costos de la compra. Usa una línea por SKU.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await BloquearAsync("Compra:" + s.SolicitudId, ct);
        var anterior = await db.Compras.SingleOrDefaultAsync(x => x.SolicitudId == s.SolicitudId, ct);
        if (anterior is not null) { if (anterior.UsuarioId != usuario) throw new ReglaNegocioException("Solicitud de otro usuario."); return anterior.Id; }
        if (!await db.Proveedores.AnyAsync(x => x.Id == s.ProveedorId && x.Activo, ct)) throw new ReglaNegocioException("Selecciona un proveedor activo.");
        var compra = new Compra { SolicitudId = s.SolicitudId, ProveedorId = s.ProveedorId, BodegaId = s.BodegaId, Referencia = s.Referencia.Trim(), UsuarioId = usuario };
        foreach (var l in s.Lineas.OrderBy(x => x.VarianteId))
        {
            var stock = await StockAsync(l.VarianteId, s.BodegaId, ct);
            stock.CostoPromedio = Promedio(stock.Cantidad, stock.CostoPromedio, l.Cantidad, l.CostoUnitario);
            stock.Cantidad = checked(stock.Cantidad + l.Cantidad);
            compra.Detalles.Add(new() { VarianteId = l.VarianteId, Cantidad = l.Cantidad, CostoUnitario = l.CostoUnitario });
            Movimiento(stock, l.Cantidad, "Compra " + compra.Referencia, usuario);
        }
        compra.Total = compra.Detalles.Sum(x => x.Cantidad * x.CostoUnitario);
        db.Add(compra); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return compra.Id;
    }

    public async Task OperarStockAsync(Guid solicitud, string tipo, int variante, int bodega, int? destino, int cantidad, string motivo, string usuario, CancellationToken ct = default)
    {
        if (solicitud == Guid.Empty || tipo is not ("Traspaso" or "Conteo" or "Revision") || cantidad < 0 || cantidad > 100000 || (tipo != "Conteo" && cantidad == 0) || string.IsNullOrWhiteSpace(motivo) || motivo.Length > 200) throw new ReglaNegocioException("Revisa operación, cantidad y motivo.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await BloquearAsync("Stock:" + solicitud, ct);
        var anterior = await db.OperacionesStock.SingleOrDefaultAsync(x => x.SolicitudId == solicitud, ct);
        if (anterior is not null) { if (anterior.UsuarioId != usuario) throw new ReglaNegocioException("Solicitud de otro usuario."); return; }
        var stock = await StockAsync(variante, bodega, ct);
        var delta = tipo == "Conteo" ? cantidad - stock.Cantidad : -cantidad;
        if (stock.Cantidad + delta < 0) throw new ReglaNegocioException("Stock insuficiente para esta operación.");
        if (tipo == "Traspaso")
        {
            if (destino is null || destino == bodega) throw new ReglaNegocioException("Selecciona otra bodega de destino.");
            var receptor = await StockAsync(variante, destino.Value, ct);
            receptor.CostoPromedio = Promedio(receptor.Cantidad, receptor.CostoPromedio, cantidad, stock.CostoPromedio);
            receptor.Cantidad = checked(receptor.Cantidad + cantidad);
            Movimiento(receptor, cantidad, "Recepción traspaso: " + motivo, usuario);
        }
        if (tipo == "Revision") db.Add(new RevisionStock { VarianteId = variante, BodegaId = bodega, Cantidad = cantidad, CostoUnitario=stock.CostoPromedio, Motivo = motivo.Trim(), UsuarioId = usuario });
        stock.Cantidad += delta;
        Movimiento(stock, delta, tipo + ": " + motivo, usuario);
        db.Add(new OperacionStock { SolicitudId = solicitud, Tipo = tipo, VarianteId = variante, BodegaId = bodega, DestinoId = tipo == "Traspaso" ? destino : null, Cantidad = delta, ConteoFisico = tipo == "Conteo" ? cantidad : null, Motivo = motivo.Trim(), UsuarioId = usuario });
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }

    public async Task ResolverRevisionAsync(int id, string estado, string motivo, string usuario, CancellationToken ct = default)
    {
        if (estado is not ("Disponible" or "Merma") || string.IsNullOrWhiteSpace(motivo) || motivo.Length > 250) throw new ReglaNegocioException("Indica destino y motivo.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await BloquearAsync("Revision:" + id, ct);
        var revision = await db.RevisionesStock.SingleOrDefaultAsync(x => x.Id == id && x.Estado == "Revision", ct) ?? throw new ReglaNegocioException("La revisión ya fue resuelta o no existe.");
        if (estado == "Disponible")
        {
            var stock = await StockAsync(revision.VarianteId, revision.BodegaId, ct);
            // Un artículo sin costo trazable vuelve como costo desconocido, no como costo cero.
            decimal? costo = revision.CostoUnitario;
            stock.CostoPromedio = Promedio(stock.Cantidad, stock.CostoPromedio, revision.Cantidad, costo);
            stock.Cantidad += revision.Cantidad;
            Movimiento(stock, revision.Cantidad, "Liberación de revisión", usuario);
        }
        revision.Estado = estado; revision.Resolucion = DateTimeOffset.UtcNow; revision.UsuarioId = usuario; revision.Motivo = motivo.Trim();
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }

    public async Task CobrarDebitoAsync(Guid solicitud, int documentoId, decimal importe, string medio, string usuario, CancellationToken ct = default)
    {
        ValidarImporte(importe);
        if (solicitud == Guid.Empty || importe == 0 || medio is not ("Efectivo" or "Tarjeta" or "Transferencia" or "Otros")) throw new ReglaNegocioException("Indica medio y monto positivo.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await BloquearCajaAsync(usuario, ct); await BloquearAsync("CobroDebito:" + documentoId, ct);
        var anterior=await db.Pagos.Include(x=>x.SesionCaja).SingleOrDefaultAsync(x=>x.SolicitudId==solicitud,ct);
        if(anterior is not null){if(anterior.SesionCaja.UsuarioId!=usuario)throw new ReglaNegocioException("Solicitud de otro usuario.");return;}
        var doc = await db.Documentos.SingleOrDefaultAsync(x => x.Id == documentoId && x.Tipo == 56, ct) ?? throw new ReglaNegocioException("La nota de débito no existe.");
        var pagado = await db.Pagos.Where(x => x.DocumentoId == documentoId).SumAsync(x => (decimal?)x.Importe, ct) ?? 0;
        if (importe > doc.Total - pagado) throw new ReglaNegocioException("El cobro supera el saldo pendiente de la nota.");
        var caja = await db.SesionesCaja.SingleOrDefaultAsync(x => x.UsuarioId == usuario && x.Cierre == null, ct) ?? throw new ReglaNegocioException("Abre tu caja para cobrar.");
        ValidarDiaCaja(caja);
        db.Add(new PagoVenta { SolicitudId = solicitud, VentaId = doc.VentaId, DocumentoId = doc.Id, SesionCajaId = caja.Id, Medio = medio, Importe = importe });
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }

    public async Task<int> CambiarAsync(Guid solicitud, int detalleId, int nuevaVariante, string medioDiferencia, string motivo, string usuario, CancellationToken ct = default)
    {
        if (solicitud == Guid.Empty || medioDiferencia is not ("Efectivo" or "Tarjeta" or "Transferencia" or "Otros")) throw new ReglaNegocioException("Selecciona un medio para la diferencia.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await BloquearCajaAsync(usuario, ct); await BloquearAsync("Cambio:" + solicitud, ct);
        var anterior = await db.Cambios.SingleOrDefaultAsync(x => x.SolicitudId == solicitud, ct);
        if (anterior is not null) { if (anterior.UsuarioId != usuario) throw new ReglaNegocioException("Solicitud de otro usuario."); return anterior.VentaNuevaId; }
        var linea = await db.DetallesVenta.Include(x => x.Venta).SingleOrDefaultAsync(x => x.Id == detalleId, ct) ?? throw new ReglaNegocioException("Venta inexistente.");
        var original = await db.Variantes.FindAsync([linea.VarianteId], ct);
        var nueva = await db.Variantes.Include(x => x.Producto).SingleOrDefaultAsync(x => x.Id == nuevaVariante && x.Activo, ct) ?? throw new ReglaNegocioException("Variante nueva inexistente.");
        if (nueva.Id == linea.VarianteId || nueva.ProductoId != original!.ProductoId) throw new ReglaNegocioException("Elige otra variante del mismo modelo para el cambio de talla o color.");
        var precio = MotorOfertas.Calcular(nueva, await new Precios(db).VigentesAsync(ct), Precios.HoyChile).Final;
        var diferencia = precio - linea.PrecioUnitario;
        var creditoId = await DevolverCoreAsync(solicitud, detalleId, 1, true, motivo, usuario, true, ct);
        var devolucion = await db.Devoluciones.SingleAsync(x => x.DocumentoId == creditoId, ct);
        var pagos = new List<PagoSolicitud> { new() { Medio = "SaldoCambio", Importe = Math.Min(precio, linea.PrecioUnitario) } };
        if (diferencia > 0) pagos.Add(new() { Medio = medioDiferencia, Importe = diferencia });
        var tipo = await db.Documentos.Where(x => x.VentaId == linea.VentaId && (x.Tipo == 33 || x.Tipo == 39)).Select(x => x.Tipo).SingleAsync(ct);
        var ventaId = await VenderCoreAsync(new() { SolicitudId = solicitud, BodegaId = linea.Venta.BodegaId, ClienteId = linea.Venta.ClienteId, TipoDocumento = tipo, Lineas = [new() { VarianteId = nueva.Id, Cantidad = 1 }], Pagos = pagos }, usuario, linea.PrecioUnitario, ct);
        if(diferencia<0&&medioDiferencia=="Efectivo")
        {
            var caja=await db.SesionesCaja.SingleAsync(x=>x.Id==devolucion.SesionCajaId,ct);
            if((await CuadraturaAsync(caja,ct)).Single(x=>x.Medio=="Efectivo").Esperado < -diferencia)throw new ReglaNegocioException("Registra fondo suficiente antes de devolver la diferencia en efectivo.");
        }
        if (diferencia < 0) db.Add(new ReembolsoPago { DevolucionId = devolucion.Id, SesionCajaId = devolucion.SesionCajaId, Medio = medioDiferencia, Importe = -diferencia });
        db.Add(new Cambio { SolicitudId = solicitud, DevolucionId = devolucion.Id, VentaNuevaId = ventaId, Diferencia = diferencia, UsuarioId = usuario });
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return ventaId;
    }
}


