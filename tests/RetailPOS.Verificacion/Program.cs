using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using RetailPOS.Application;
using RetailPOS.Domain;
using RetailPOS.Infrastructure;

// Cada ejecución usa una base aislada, nunca la base de trabajo ni las anteriores.
var nombre = "RetailPOS_Test_" + Guid.NewGuid().ToString("N");
var conexion = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("RETAILPOS_TEST_SQL") ?? "Server=localhost;Integrated Security=True;Encrypt=True;TrustServerCertificate=True") { InitialCatalog = nombre };
DbContextOptions<TiendaDb> Opciones() => new DbContextOptionsBuilder<TiendaDb>().UseSqlServer(conexion.ConnectionString).Options;
void Verificar(bool condicion, string mensaje) { if (!condicion) throw new Exception(mensaje); Console.WriteLine("OK: " + mensaje); }
async Task DebeRechazar(Func<Task> accion, string mensaje)
{
    try { await accion(); } catch (ReglaNegocioException) { Console.WriteLine("OK: " + mensaje); return; }
    throw new Exception("No se rechazó: " + mensaje);
}
try
{
    await MigracionVerificacion.EjecutarAsync(conexion.ConnectionString);
    await using (var db = new TiendaDb(Opciones()))
    {
        await db.Database.MigrateAsync();
        db.AddRange(new Marca { Nombre = "Prueba" }, new TallaCalzado { Valor = "40", Sistema = "EU" }, new TallaCalzado { Valor = "41", Sistema = "EU" });
        await db.SaveChangesAsync();
        var producto = new Producto { Marca = "Prueba", Modelo = "Carrera" };
        var variante = new Variante { Producto = producto, Sku = "TEST-40", Color = "Negro", Talla = "40", Precio = 50000 };
        var otra = new Variante { Producto = producto, Sku = "TEST-41", Color = "Negro", Talla = "41", Precio = 55000 };
        db.AddRange(variante, otra, new Bodega { Nombre = "Prueba" });
        await db.SaveChangesAsync();
        var ops = new Operaciones(db);
        await ops.MoverStockAsync(1, 1, 3, "Ingreso de prueba", "usuario");
        await ops.MoverStockAsync(2, 1, 1, "Última unidad", "usuario");
        await ops.AbrirCajaAsync(10000, "usuario");
    }
    var solicitud = new SolicitudVenta { BodegaId = 1, Lineas = [new() { VarianteId = 1, Cantidad = 2 }] };
    int ventaId;
    await using (var db = new TiendaDb(Opciones())) ventaId = await new Operaciones(db).VenderAsync(solicitud, "usuario");
    await using (var db = new TiendaDb(Opciones()))
    {
        var repetida = await new Operaciones(db).VenderAsync(solicitud, "usuario");
        Verificar(repetida == ventaId && await db.Ventas.CountAsync() == 1 && await db.Existencias.Where(x => x.VarianteId == 1).Select(x => x.Cantidad).SingleAsync() == 1, "Reintentar venta no duplica venta ni salida");
    }
    await using (var db = new TiendaDb(Opciones()))
    {
        await DebeRechazar(async () => await new Operaciones(db).VenderAsync(new() { BodegaId = 1, Lineas = [new() { VarianteId = 1, Cantidad = 2 }] }, "usuario"), "Venta sin stock se rechaza");
    }
    await using (var db = new TiendaDb(Opciones())) Verificar(await db.Existencias.Where(x => x.VarianteId == 1).Select(x => x.Cantidad).SingleAsync() == 1, "Venta rechazada conserva existencias");

    async Task<bool> UltimaUnidad()
    {
        await using var db = new TiendaDb(Opciones());
        try { await new Operaciones(db).VenderAsync(new() { BodegaId = 1, Lineas = [new() { VarianteId = 2, Cantidad = 1 }] }, "usuario"); return true; }
        catch (ReglaNegocioException) { return false; }
        catch (SqlException e) when (e.Number == 1205) { return false; }
        catch (DbUpdateException e) when (e.InnerException is SqlException sql && sql.Number == 1205) { return false; }
    }
    var concurrentes = await Task.WhenAll(UltimaUnidad(), UltimaUnidad());
    Verificar(concurrentes.Count(x => x) == 1, "Solo una venta consume la última unidad en concurrencia");
    int referencia; int lineaId;
    await using (var db = new TiendaDb(Opciones()))
    {
        referencia = await db.Documentos.Where(x => x.VentaId == ventaId && x.Tipo == 39).Select(x => x.Id).SingleAsync();
        lineaId = await db.DetallesVenta.Where(x => x.VentaId == ventaId).Select(x => x.Id).SingleAsync();
        await new Operaciones(db).CrearDocumentoAsync(ventaId, 52, referencia, "Despacho", "usuario");
    }
    await using (var db = new TiendaDb(Opciones())) Verificar(await db.Existencias.Where(x => x.VarianteId == 1).Select(x => x.Cantidad).SingleAsync() == 1, "Guía no duplica salida de stock");
    var devolucionId = Guid.NewGuid(); int credito;
    await using (var db = new TiendaDb(Opciones())) credito = await new Operaciones(db).DevolverAsync(devolucionId, lineaId, 1, true, "Cambio de talla", "usuario");
    await using (var db = new TiendaDb(Opciones()))
    {
        Verificar(await new Operaciones(db).DevolverAsync(devolucionId, lineaId, 1, true, "Cambio de talla", "usuario") == credito, "Devolución idempotente");
        Verificar(await db.Existencias.Where(x => x.VarianteId == 1).Select(x => x.Cantidad).SingleAsync() == 2, "Devolución repone una unidad");
        var doc = await db.Documentos.Include(x => x.Detalles).SingleAsync(x => x.Id == credito);
        Verificar(doc.Tipo == 61 && doc.Total == 50000 && doc.Detalles.Single().Cantidad == 1, "Nota de crédito refleja solo la devolución parcial");
    }
    await using (var db = new TiendaDb(Opciones())) await DebeRechazar(async () => await new Operaciones(db).DevolverAsync(Guid.NewGuid(), lineaId, 2, true, "Exceso", "usuario"), "No se permite devolver más que lo pendiente");
    var debitoId = Guid.NewGuid();
    await using (var db = new TiendaDb(Opciones())) await new Operaciones(db).DebitarAsync(debitoId, referencia, 1000, "Ajuste", "usuario");
    await using (var db = new TiendaDb(Opciones()))
    {
        await new Operaciones(db).DebitarAsync(debitoId, referencia, 1000, "Ajuste", "usuario");
        Verificar(await db.Documentos.CountAsync(x => x.Tipo == 56) == 1, "Nota de débito idempotente");
        await new Operaciones(db).CerrarCajaAsync(115000, "usuario");
        var caja = await db.SesionesCaja.SingleAsync();
        Verificar(caja.EfectivoEsperado == 115000 && caja.EfectivoContado == 115000, "Caja descuenta reembolso y excluye débito sin cobro");
    }
    await using (var db = new TiendaDb(Opciones())) await DebeRechazar(async () => await new Operaciones(db).VenderAsync(new() { BodegaId = 1, Lineas = [new() { VarianteId = 1, Cantidad = 1 }] }, "usuario"), "Caja cerrada impide nuevas ventas");
    await ComercialVerificacion.EjecutarAsync(Opciones());
    await HttpVerificacion.EjecutarAsync(conexion.ConnectionString);
    await CompletaVerificacion.EjecutarAsync(Opciones());
    await CuadraturaDiariaVerificacion.EjecutarAsync(Opciones());
    Console.WriteLine("Verificación comercial finalizada correctamente.");
}
finally
{
    if (!nombre.StartsWith("RetailPOS_Test_", StringComparison.Ordinal) || conexion.InitialCatalog != nombre) throw new InvalidOperationException("Nombre de base de prueba inseguro.");
    await using var limpieza = new TiendaDb(Opciones());
    await limpieza.Database.EnsureDeletedAsync();
}


