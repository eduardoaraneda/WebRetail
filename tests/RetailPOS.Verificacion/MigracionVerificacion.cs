using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RetailPOS.Infrastructure;

internal static class MigracionVerificacion
{
    public static async Task EjecutarAsync(string conexionBase)
    {
        var conexion = new SqlConnectionStringBuilder(conexionBase);
        var nombre = "RetailPOS_Test_Upgrade_" + Guid.NewGuid().ToString("N");
        conexion.InitialCatalog = nombre;
        var opciones = new DbContextOptionsBuilder<TiendaDb>().UseSqlServer(conexion.ConnectionString).Options;
        try
        {
            await using var db = new TiendaDb(opciones);
            await db.GetService<IMigrator>().MigrateAsync("20261114211238_Inicial");
            await db.Database.ExecuteSqlRawAsync("""
                INSERT Productos (Marca, Modelo, Activo) VALUES (N'Marca anterior',N'Modelo anterior',1);
                INSERT Variantes (ProductoId,Sku,Color,Talla,SistemaTalla,Precio,Activo) VALUES (1,N'ANT-40',N'Negro',N'40',N'EU',69990,1);
                INSERT Bodegas (Nombre) VALUES (N'Bodega anterior');
                INSERT SesionesCaja (UsuarioId,Apertura,Cierre,FondoInicial,EfectivoEsperado,EfectivoContado) VALUES (N'prueba',SYSDATETIMEOFFSET(),SYSDATETIMEOFFSET(),0,69990,69990);
                INSERT Ventas (SolicitudId,Fecha,BodegaId,SesionCajaId,UsuarioId,Total,MedioPago) VALUES (NEWID(),SYSDATETIMEOFFSET(),1,1,N'prueba',69990,N'Efectivo');
                INSERT DetallesVenta (VentaId,VarianteId,Descripcion,Cantidad,PrecioUnitario) VALUES (1,1,N'Venta anterior',1,69990);
                """);
            await db.Database.MigrateAsync();
            if (!await db.Marcas.AnyAsync(x => x.Nombre == "Marca anterior") || !await db.Tallas.AnyAsync(x => x.Valor == "40" && x.Sistema == "EU") || await db.DetallesVenta.Select(x => x.PrecioLista).SingleAsync() != 69990 || await db.Cuadraturas.AnyAsync())
                throw new Exception("La migración no conserva los datos históricos.");
            Console.WriteLine("OK MIGRACIÓN: Marcas, tallas y precios anteriores conservados; no se inventan cuadraturas históricas");
            if(await db.Pagos.SumAsync(x=>x.Importe)!=69990 || await db.DetallesVenta.AnyAsync(x=>x.CostoUnitario!=null)) throw new Exception("Migración de pagos o costos históricos incorrecta.");
            Console.WriteLine("OK MIGRACIÓN: Pagos anteriores recuperados; costos desconocidos siguen vacíos");
        }
        finally
        {
            if (conexion.InitialCatalog != nombre || !nombre.StartsWith("RetailPOS_Test_Upgrade_", StringComparison.Ordinal)) throw new Exception("Base de prueba inválida.");
            await using var db = new TiendaDb(opciones);
            await db.Database.EnsureDeletedAsync();
        }
    }
}

