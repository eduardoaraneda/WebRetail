using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Domain;
using RetailPOS.Infrastructure;
namespace RetailPOS.Web;

public sealed class RespaldosService(TiendaDb db)
{
    public async Task<string> CrearArchivoAsync()
    {
        var cadena = db.Database.GetConnectionString()!;
        var datos = new SqlConnectionStringBuilder(cadena);
        if (datos.InitialCatalog is not ("RetailPOS" or "ZapatillasPOS") && !datos.InitialCatalog.StartsWith("RetailPOS_Test_",StringComparison.Ordinal)) throw new ReglaNegocioException("Base no autorizada para respaldo.");
        await using var sql = new SqlConnection(cadena); await sql.OpenAsync();
        using var rutaCmd = new SqlCommand("SELECT CONVERT(nvarchar(500),SERVERPROPERTY('InstanceDefaultBackupPath'))",sql);
        var directorio = (string?)await rutaCmd.ExecuteScalarAsync();
        if (string.IsNullOrWhiteSpace(directorio)) throw new ReglaNegocioException("SQL Server no tiene directorio de respaldo configurado.");
        var ruta = directorio.TrimEnd('\\','/') + "\\RetailPOS_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N") + ".bak";
        using var backup = new SqlCommand("BACKUP DATABASE " + Sql(datos.InitialCatalog) + " TO DISK=@ruta WITH COPY_ONLY,CHECKSUM; RESTORE VERIFYONLY FROM DISK=@ruta WITH CHECKSUM;", sql) { CommandTimeout = 600 };
        backup.Parameters.AddWithValue("@ruta",ruta); await backup.ExecuteNonQueryAsync();
        return ruta;
    }
    public async Task<int> CrearAsync(string usuario)
    {
        var ruta=await CrearArchivoAsync();
        var registro = new Respaldo { Ruta = ruta, UsuarioId = usuario }; db.Add(registro); await db.SaveChangesAsync(); return registro.Id;
    }
    public async Task ProbarAsync(int id)
    {
        var respaldo = await db.Respaldos.FindAsync(id) ?? throw new ReglaNegocioException("Respaldo inexistente.");
        var cadena = new SqlConnectionStringBuilder(db.Database.GetConnectionString()!) { InitialCatalog = "master" };
        var nombre = "RetailPOS_Recovery_" + Guid.NewGuid().ToString("N");
        await using var sql = new SqlConnection(cadena.ConnectionString); await sql.OpenAsync();
        using var rutas = new SqlCommand("SELECT CONVERT(nvarchar(500),SERVERPROPERTY('InstanceDefaultDataPath')),CONVERT(nvarchar(500),SERVERPROPERTY('InstanceDefaultLogPath'))",sql);
        string data, log;
        await using (var reader = await rutas.ExecuteReaderAsync()) { await reader.ReadAsync(); data = reader.GetString(0); log = reader.GetString(1); }
        var archivos = new List<(string Nombre,string Tipo)>();
        using (var lista = new SqlCommand("RESTORE FILELISTONLY FROM DISK=@ruta",sql))
        {
            lista.Parameters.AddWithValue("@ruta",respaldo.Ruta);
            await using var reader = await lista.ExecuteReaderAsync();
            while (await reader.ReadAsync()) archivos.Add((reader.GetString(reader.GetOrdinal("LogicalName")),reader.GetString(reader.GetOrdinal("Type"))));
        }
        try
        {
            using var restore = new SqlCommand { Connection = sql, CommandTimeout = 600 };
            restore.Parameters.AddWithValue("@ruta",respaldo.Ruta);
            var moves = new List<string>();
            for (var i=0;i<archivos.Count;i++)
            {
                if (archivos[i].Tipo is not ("D" or "L")) throw new ReglaNegocioException("Tipo de archivo SQL no soportado por esta prueba.");
                restore.Parameters.AddWithValue("@logico"+i,archivos[i].Nombre);
                restore.Parameters.AddWithValue("@fisico"+i,(archivos[i].Tipo=="L" ? log : data).TrimEnd('\\','/') + "\\" + nombre + "_" + i + (archivos[i].Tipo=="L" ? ".ldf" : ".mdf"));
                moves.Add($"MOVE @logico{i} TO @fisico{i}");
            }
            restore.CommandText = $"RESTORE DATABASE {Sql(nombre)} FROM DISK=@ruta WITH CHECKSUM,RECOVERY,{string.Join(',',moves)}; DBCC CHECKDB ({Sql(nombre)}) WITH NO_INFOMSGS;";
            await restore.ExecuteNonQueryAsync();
            using var comprobar = new SqlCommand($"SELECT COUNT(*) FROM {Sql(nombre)}.sys.tables WHERE name IN ('Ventas','Existencias','AspNetUsers','__EFMigrationsHistory')",sql);
            if (Convert.ToInt32(await comprobar.ExecuteScalarAsync()) != 4) throw new ReglaNegocioException("El respaldo no contiene el esquema esperado.");
            respaldo.RestauracionProbada = DateTimeOffset.UtcNow; respaldo.Estado = "Restauración comprobada"; await db.SaveChangesAsync();
        }
        finally
        {
            if (!nombre.StartsWith("RetailPOS_Recovery_",StringComparison.Ordinal) || nombre.Length != "RetailPOS_Recovery_".Length+32) throw new InvalidOperationException("Nombre de recuperación inválido.");
            using var limpiar = new SqlCommand($"IF DB_ID(@nombre) IS NOT NULL BEGIN ALTER DATABASE {Sql(nombre)} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE {Sql(nombre)}; END",sql) { CommandTimeout = 120 };
            limpiar.Parameters.AddWithValue("@nombre",nombre); await limpiar.ExecuteNonQueryAsync();
        }
    }
    private static string Sql(string nombre)
    {
        if (nombre.Any(x => !char.IsLetterOrDigit(x) && x != '_')) throw new ReglaNegocioException("Identificador de base no permitido.");
        return "["+nombre+"]";
    }
}

