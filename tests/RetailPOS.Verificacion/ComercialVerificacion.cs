using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Domain;
using RetailPOS.Infrastructure;

internal static class ComercialVerificacion
{
    public static async Task EjecutarAsync(DbContextOptions<TiendaDb> opciones)
    {
        void Verificar(bool ok, string mensaje) { if (!ok) throw new Exception(mensaje); Console.WriteLine("OK COMERCIAL: " + mensaje); }
        var hoy = Precios.HoyChile;
        var marca = new Marca { Id = 1, Nombre = "Prueba" };
        var talla = new TallaCalzado { Id = 1, Valor = "40", Sistema = "EU" };
        var variante = new Variante { Id = 1, Precio = 50000, Talla = "40", SistemaTalla = "EU", Producto = new() { Marca = "Prueba" } };
        Oferta Oferta(int id, decimal valor, string tipo = "Porcentaje") => new() { Id = id, Nombre = "Oferta " + id, Inicio = hoy, Fin = hoy, Valor = valor, Tipo = tipo };
        var porMarca = Oferta(1, 10); porMarca.MarcaId = 1; porMarca.Marca = marca;
        var porTalla = Oferta(2, 20); porTalla.TallaId = 1; porTalla.Talla = talla;
        var porSku = Oferta(3, 15000, "Monto"); porSku.VarianteId = 1;
        Verificar(MotorOfertas.Calcular(variante, [porMarca], hoy).Final == 45000, "Oferta por marca");
        Verificar(MotorOfertas.Calcular(variante, [porTalla], hoy).Final == 40000, "Oferta por talla");
        Verificar(MotorOfertas.Calcular(variante, [porMarca, porTalla, porSku], hoy).Final == 35000, "Mejor precio entre ofertas sin acumular");
        var combinada = Oferta(4, 90); combinada.MarcaId = 1; combinada.Marca = marca; combinada.TallaId = 99; combinada.Talla = new() { Id = 99, Valor = "40", Sistema = "US" };
        Verificar(MotorOfertas.Calcular(variante, [combinada], hoy).Final == 50000, "Filtros combinados y sistema de talla se respetan");
        porSku.Fin = hoy.AddDays(-1);
        Verificar(MotorOfertas.Calcular(variante, [porSku], hoy).Final == 50000, "Oferta vencida no aplica");
        porSku.Fin = hoy; porSku.Inicio = hoy.AddDays(1);
        Verificar(MotorOfertas.Calcular(variante, [porSku], hoy).Final == 50000, "Oferta futura no aplica");
        porSku.Inicio = hoy; porSku.Activa = false;
        Verificar(MotorOfertas.Calcular(variante, [porSku], hoy).Final == 50000, "Oferta desactivada no aplica");
        var limite = Oferta(5, 100); limite.VarianteId = 1;
        Verificar(MotorOfertas.Calcular(variante, [limite], hoy).Final == 1, "Descuento no deja precios negativos ni cero");
        variante.Precio = 101; limite.Valor = 50;
        Verificar(MotorOfertas.Calcular(variante, [limite], hoy).Final == 51, "Redondeo del precio final a CLP");
        porSku.Activa = true;
        // Las entidades maestras ya existen en esta base: solo guardar sus referencias.
        porMarca.Marca = null; porTalla.Talla = null;
        porMarca.Id = 0; porTalla.Id = 0; porSku.Id = 0;
        await using (var db = new TiendaDb(opciones))
        {
            db.AddRange(porMarca, porTalla, porSku); await db.SaveChangesAsync();
            await new Operaciones(db).AbrirCajaAsync(10000, "comercial");
        }
        var solicitudMovimiento = Guid.NewGuid();
        await using (var db = new TiendaDb(opciones)) await new Operaciones(db).MovimientoCajaAsync(solicitudMovimiento, "Ingreso", 5000, "Fondo adicional", "comercial");
        await using (var db = new TiendaDb(opciones))
        {
            var ops = new Operaciones(db);
            await ops.MovimientoCajaAsync(solicitudMovimiento, "Ingreso", 5000, "Fondo adicional", "comercial");
            await ops.MovimientoCajaAsync(Guid.NewGuid(), "Retiro", 2000, "Retiro parcial", "comercial");
            Verificar(await db.MovimientosCaja.CountAsync() == 2, "Ingreso y retiro, sin duplicar reintento");
        }
        await using (var db = new TiendaDb(opciones))
        {
            try { await new Operaciones(db).MovimientoCajaAsync(Guid.NewGuid(), "Retiro", 999999, "Exceso", "comercial"); throw new Exception("Se permitió retirar más que el efectivo esperado"); }
            catch (ReglaNegocioException) { Console.WriteLine("OK COMERCIAL: Retiro excesivo rechazado"); }
        }
        int ventaId;
        await using (var db = new TiendaDb(opciones)) ventaId = await new Operaciones(db).VenderAsync(new() { BodegaId = 1, MedioPago = "Tarjeta", Lineas = [new() { VarianteId = 1, Cantidad = 1 }] }, "comercial");
        int detalleId;
        await using (var db = new TiendaDb(opciones))
        {
            var linea = await db.DetallesVenta.SingleAsync(x => x.VentaId == ventaId);
            detalleId = linea.Id;
            Verificar(linea.PrecioLista == 50000 && linea.PrecioUnitario == 35000 && linea.DescuentoUnitario == 15000 && linea.OfertaNombre is not null, "Venta guarda precio de lista, descuento y oferta");
            await db.Ofertas.ExecuteUpdateAsync(s => s.SetProperty(x => x.Activa, false));
            var v = await db.Variantes.FindAsync(1); v!.Precio = 100000;
            await db.SaveChangesAsync();
        }
        await using (var db = new TiendaDb(opciones))
        {
            try { await new Operaciones(db).CerrarCajaDetalladaAsync(new() { Contado = 12000, Tarjeta = 35000, Transferencia = 0 }, "comercial"); throw new Exception("Se permitió descuadre sin observación"); }
            catch (ReglaNegocioException) { Console.WriteLine("OK COMERCIAL: Descuadre exige observación"); }
        }
        await using (var db = new TiendaDb(opciones))
        {
            Verificar(await db.SesionesCaja.AnyAsync(x => x.UsuarioId == "comercial" && x.Cierre == null), "Cierre rechazado conserva caja abierta");
            await new Operaciones(db).CerrarCajaDetalladaAsync(new() { Contado = 12000, Tarjeta = 35000, Transferencia = 0, Observacion = "Faltante de efectivo 1000" }, "comercial");
            var caja = await db.SesionesCaja.Include(x => x.Cuadraturas).SingleAsync(x => x.UsuarioId == "comercial");
            Verificar(caja.Cuadraturas.Count == 4 && caja.EfectivoEsperado == 13000 && caja.EfectivoContado == 12000 && caja.Cuadraturas.Single(x => x.Medio == "Tarjeta").Esperado == 35000, "Cuadratura conserva cuatro medios y faltante");
        }
        await using (var db = new TiendaDb(opciones)) await new Operaciones(db).AbrirCajaAsync(0, "comercial");
        await using (var db = new TiendaDb(opciones))
        {
            await new Operaciones(db).DevolverAsync(Guid.NewGuid(), detalleId, 1, true, "Devolución de venta con oferta", "comercial");
            var devolucion = await db.Devoluciones.SingleAsync(x => x.DetalleVentaId == detalleId);
            Verificar(devolucion.Reembolso == 35000, "Reembolso conserva precio ofertado aunque cambie la promoción");
        }
        await using (var db = new TiendaDb(opciones))
        {
            await new Operaciones(db).CerrarCajaDetalladaAsync(new() { Contado = 0, Tarjeta = -35000, Transferencia = 0 }, "comercial");
            var filas = await db.Cuadraturas.Where(x => x.SesionCaja.UsuarioId == "comercial" && x.SesionCaja.FondoInicial == 0).ToListAsync();
            Verificar(filas.Count == 4 && filas.All(x => x.Esperado == x.Contado), "Cuadratura admite saldo tarjeta negativo por devolución de otra sesión");
        }
    }
}


