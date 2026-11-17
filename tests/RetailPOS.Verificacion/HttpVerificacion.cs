using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Infrastructure;

internal static class HttpVerificacion
{
    public static async Task EjecutarAsync(string conexion)
    {
        using var reserva = new TcpListener(IPAddress.Loopback, 0);
        reserva.Start();
        var puerto = ((IPEndPoint)reserva.LocalEndpoint).Port;
        reserva.Stop();
        var web = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/RetailPOS.Web"));
        var inicio = new ProcessStartInfo("dotnet") { WorkingDirectory = web, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        inicio.ArgumentList.Add(Path.Combine(web, "bin/Debug/net11.0/RetailPOS.Web.dll"));
        inicio.ArgumentList.Add("--urls"); inicio.ArgumentList.Add($"https://localhost:{puerto}");
        inicio.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        inicio.Environment["ConnectionStrings__Tienda"] = conexion;
        inicio.Environment["Acceso:LimiteIntentos"]="200";
        var correos=Path.Combine(Path.GetTempPath(),"RetailPOS_Correos_"+Guid.NewGuid().ToString("N"));
        inicio.Environment["Correo:Directorio"]=correos;
        inicio.Environment["Correo:UrlPublica"]=$"https://localhost:{puerto}";
        using var proceso = Process.Start(inicio) ?? throw new Exception("No se pudo iniciar la aplicación de prueba.");
        var salida = proceso.StandardOutput.ReadToEndAsync();
        var errores = proceso.StandardError.ReadToEndAsync();
        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        using var http = new HttpClient(handler) { BaseAddress = new Uri($"https://localhost:{puerto}"), Timeout = TimeSpan.FromSeconds(20) };
        void Verificar(bool ok, string texto) { if (!ok) throw new Exception(texto); Console.WriteLine("OK HTTP: " + texto); }
        async Task<string> Pagina(string ruta)
        {
            using var response = await http.GetAsync(ruta);
            var html = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK) throw new Exception($"GET {ruta}: {(int)response.StatusCode}");
            return html;
        }
        async Task<HttpResponseMessage> Post(string ruta, string pagina, Dictionary<string, string> valores)
        {
            var html = await Pagina(pagina);
            var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
            if (!match.Success) throw new Exception("Falta antiforgery: " + pagina);
            valores["__RequestVerificationToken"] = WebUtility.HtmlDecode(match.Groups[1].Value);
            return await http.PostAsync(ruta, new FormUrlEncodedContent(valores));
        }
        try
        {
            var listo = false;
            for (var i = 0; i < 40; i++)
            {
                if (proceso.HasExited) throw new Exception("El servidor de prueba terminó: " + await errores);
                try { using var prueba = await http.GetAsync("/Acceso/Login"); listo = prueba.StatusCode == HttpStatusCode.OK; } catch (HttpRequestException) { }
                if (listo) break;
                await Task.Delay(250);
            }
            Verificar(listo, "Servidor HTTPS disponible");
            using (var anon = await http.GetAsync("/Inventario")) Verificar(anon.StatusCode == HttpStatusCode.Redirect, "Rutas comerciales requieren acceso");
            var password = "Z!a9" + Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
            using (var setup = await Post("/Acceso/Configurar", "/Acceso/Configurar", new() { ["Email"] = "admin@prueba.invalid", ["Password"] = password })) Verificar(setup.StatusCode == HttpStatusCode.Redirect, "Creación inicial de administrador");
            using (var segunda = await http.GetAsync("/Acceso/Configurar")) Verificar(segunda.StatusCode == HttpStatusCode.NotFound, "Configuración inicial se deshabilita");
            using(var acceso=await http.GetAsync("/Acceso/Administradores"))Verificar(acceso.StatusCode==HttpStatusCode.Redirect&&acceso.Headers.Location!.ToString().Contains("administrador=True",StringComparison.OrdinalIgnoreCase),"Crear administradores requiere acceso autorizado");
            using(var registro=await Post("/Acceso/Registro","/Acceso/Registro",new(){["Email"]="pendiente@prueba.invalid",["Password"]=password,["Confirmar"]=password,["Rol"]="Administrador",["Activo"]="true"}))Verificar(registro.StatusCode==HttpStatusCode.Redirect,"Registro público crea solicitud de cuenta");
            await using(var registroDb=new TiendaDb(new DbContextOptionsBuilder<TiendaDb>().UseSqlServer(conexion).Options))
            {
                var pendiente=await registroDb.Users.SingleAsync(x=>x.Email=="pendiente@prueba.invalid");var rolesCuenta=await registroDb.UserRoles.Where(x=>x.UserId==pendiente.Id).Join(registroDb.Roles,r=>r.RoleId,r=>r.Id,(ur,r)=>r.Name).ToListAsync();
                Verificar(!pendiente.Activo&&rolesCuenta.SequenceEqual(new[]{"Vendedor"}),"Registro no permite autoasignarse administrador ni activarse");
            }
            using(var denegado=await Post("/Acceso/Login","/Acceso/Login",new(){["Email"]="pendiente@prueba.invalid",["Password"]=password}))Verificar(denegado.StatusCode==HttpStatusCode.OK,"Cuenta pendiente no puede iniciar sesión");
            using(var validacion=await Post("/Acceso/Login","/Acceso/Login",new(){["Email"]="no-es-correo",["Password"]=""}))
            {
                var html=WebUtility.HtmlDecode(await validacion.Content.ReadAsStringAsync());
                Verificar(html.Contains("correo electrónico válido")&&html.Contains("obligatorio")&&!html.Contains("field is required"),"Validación de acceso en español");
            }
            using (var login = await Post("/Acceso/Login", "/Acceso/Login", new() { ["Email"] = "admin@prueba.invalid", ["Password"] = password,["administrador"]="true",["returnUrl"]="/Usuarios" }))
            {
                Verificar(login.StatusCode == HttpStatusCode.Redirect, "Login de administrador");
                Verificar(login.Headers.Location?.ToString()=="/Usuarios","Acceso administrativo conserva su destino local");
                Verificar(login.Headers.TryGetValues("Set-Cookie", out var cookies) && cookies.Any(c => c.Contains("httponly", StringComparison.OrdinalIgnoreCase) && c.Contains("secure", StringComparison.OrdinalIgnoreCase)), "Cookie Secure y HttpOnly");
            }
            foreach (var ruta in new[] { "/", "/Catalogo", "/Inventario", "/Clientes", "/Ventas", "/Ventas/Nueva", "/Ventas/Detalle/1", "/Caja", "/Caja/Detalle/1", "/Documentos", "/Documentos/Detalle/1", "/Usuarios", "/Reportes", "/Parametros", "/Ofertas", "/Compras", "/Compras/Nueva", "/Proveedores", "/Proveedores/Editar", "/InventarioControl", "/Pagos", "/Administracion", "/Administracion/Auditoria" })
            {
                await Pagina(ruta); Console.WriteLine("OK HTTP: " + ruta);
            }
            Verificar((await Pagina("/Catalogo")).Contains("class=\"sidebar-link is-active\" aria-current=\"page\""),"Menú lateral marca la sección actual");
            var cuadratura=WebUtility.HtmlDecode(await Pagina("/Caja"));
            Verificar(cuadratura.Contains("Total boletas y facturas")&&cuadratura.Contains("Total neto ingresado por el cajero")&&cuadratura.Contains("Otros medios"),"Cuadratura diaria muestra documentos, medios y comparación del cajero");
            if(Environment.GetEnvironmentVariable("RETAILPOS_QA_SNAPSHOT")=="1")
            {
                var qa=Path.GetFullPath(Path.Combine(web,"../../artifacts/qa"));Directory.CreateDirectory(qa);
                var vista=Regex.Replace(await Pagina("/"),"<input[^>]*name=\"__RequestVerificationToken\"[^>]*>","",RegexOptions.IgnoreCase);
                await File.WriteAllTextAsync(Path.Combine(qa,"panel-ejemplo.html"),vista);
                foreach(var archivo in new[]{"css/site.css","css/shell.css","js/interfaz.js"}){var destino=Path.Combine(qa,archivo);Directory.CreateDirectory(Path.GetDirectoryName(destino)!);File.Copy(Path.Combine(web,"wwwroot",archivo),destino,true);}
            }
            using (var csrf = await http.PostAsync("/Inventario/Movimiento", new FormUrlEncodedContent(new Dictionary<string, string> { ["varianteId"] = "1", ["bodegaId"] = "1", ["cantidad"] = "100", ["motivo"] = "Sin token" }))) Verificar(csrf.StatusCode == HttpStatusCode.BadRequest, "POST sin antiforgery rechazado");
            using(var excel=await http.GetAsync("/Reportes/Excel"))
            {
                var bytes=await excel.Content.ReadAsByteArrayAsync();
                using var libro=new ClosedXML.Excel.XLWorkbook(new MemoryStream(bytes));
                Verificar(excel.StatusCode==HttpStatusCode.OK&&libro.Worksheets.First().Cell(1,1).GetString()=="Venta","Exportación Excel abre como libro válido");
            }
            using (var marca = await Post("/Parametros/Marca", "/Parametros", new() { ["Nombre"] = "HTTP" })) Verificar(marca.StatusCode == HttpStatusCode.Redirect, "Creación de marca");
            using (var talla = await Post("/Parametros/Talla", "/Parametros", new() { ["Valor"] = "42", ["Sistema"] = "EU" })) Verificar(talla.StatusCode == HttpStatusCode.Redirect, "Creación de talla");
            using (var producto = await Post("/Catalogo/Producto", "/Catalogo", new() { ["Marca"] = "HTTP", ["Modelo"] = "Modelo de prueba" })) Verificar(producto.StatusCode == HttpStatusCode.Redirect, "Alta de producto por formulario");
            using (var variante = await Post("/Catalogo/Variante", "/Catalogo", new() { ["ProductoId"] = "1", ["Sku"] = "HTTP-42", ["Color"] = "Azul", ["TallaId"] = "3", ["Precio"] = "60000" })) Verificar(variante.StatusCode == HttpStatusCode.Redirect, "Alta de variante por formulario");
            var hoy = RetailPOS.Infrastructure.Precios.HoyChile.ToString("yyyy-MM-dd");
            using (var oferta = await Post("/Ofertas/Crear", "/Ofertas", new() { ["Nombre"] = "Oferta HTTP", ["VarianteId"] = "1", ["Tipo"] = "Porcentaje", ["Valor"] = "10", ["Inicio"] = hoy, ["Fin"] = hoy })) Verificar(oferta.StatusCode == HttpStatusCode.Redirect, "Creación de oferta por formulario");
            using(var proveedor=await Post("/Proveedores/Editar","/Proveedores/Editar",new(){["Nombre"]="Proveedor HTTP",["Activo"]="true"}))Verificar(proveedor.StatusCode==HttpStatusCode.Redirect,"Proveedor creado por formulario");
            var solicitudCompra=Guid.NewGuid();
            using(var compra=await Post("/Compras/Nueva","/Compras/Nueva",new(){["SolicitudId"]=solicitudCompra.ToString(),["ProveedorId"]="1",["BodegaId"]="1",["Referencia"]="HTTP-COMPRA",["Lineas[0].VarianteId"]="1",["Lineas[0].Cantidad"]="1",["Lineas[0].CostoUnitario"]="123.45"}))Verificar(compra.StatusCode==HttpStatusCode.Redirect,"Compra con costo decimal por formulario");
            await using(var compraDb=new TiendaDb(new DbContextOptionsBuilder<TiendaDb>().UseSqlServer(conexion).Options))Verificar((await compraDb.Compras.Include(x=>x.Detalles).SingleAsync(x=>x.SolicitudId==solicitudCompra)).Detalles.Single().CostoUnitario==123.45m,"Importes HTML conservan decimales independientemente del idioma");
            using (var caja = await Post("/Caja/Abrir", "/Caja", new() { ["fondo"] = "0" })) Verificar(caja.StatusCode == HttpStatusCode.Redirect, "Apertura de caja por formulario");
            using (var venta = await Post("/Ventas/Nueva", "/Ventas/Nueva", new() { ["SolicitudId"] = Guid.NewGuid().ToString(), ["BodegaId"] = "1", ["TipoDocumento"] = "39", ["MedioPago"] = "Efectivo", ["Lineas[0].VarianteId"] = "1", ["Lineas[0].Cantidad"] = "1" })) Verificar(venta.StatusCode == HttpStatusCode.Redirect, "Venta completa por formulario");
            using (var cierre = await Post("/Caja/Cerrar", "/Caja", new() { ["Contado"] = "0", ["Tarjeta"] = "0", ["Transferencia"] = "0", ["Observacion"] = "Conteo con diferencia de prueba" })) Verificar(cierre.StatusCode == HttpStatusCode.Redirect, "Cierre con cuadratura por formulario");
            await using (var revisionDb = new TiendaDb(new DbContextOptionsBuilder<TiendaDb>().UseSqlServer(conexion).Options))
            {
                var uid = await revisionDb.Users.Where(x => x.Email == "admin@prueba.invalid").Select(x => x.Id).SingleAsync();
                var cajaId = await revisionDb.SesionesCaja.Where(x => x.UsuarioId == uid).Select(x => x.Id).SingleAsync();
                using var revision = await Post("/Caja/Revisar", "/Caja/Detalle/" + cajaId, new() { ["id"] = cajaId.ToString() });
                Verificar(revision.StatusCode == HttpStatusCode.Redirect && await revisionDb.SesionesCaja.AnyAsync(x => x.Id == cajaId && x.Revision != null && x.RevisadoPor == uid), "Administrador revisa y deja trazabilidad del cierre");
            }
            using (var usuario = await Post("/Usuarios/Crear", "/Usuarios", new() { ["Email"] = "vendedor@prueba.invalid", ["Password"] = password, ["Rol"] = "Vendedor" })) Verificar(usuario.StatusCode == HttpStatusCode.Redirect, "Creación de vendedor");
            string vendedorId;
            await using(var usuariosDb=new TiendaDb(new DbContextOptionsBuilder<TiendaDb>().UseSqlServer(conexion).Options))
            {
                vendedorId=await usuariosDb.Users.Where(x=>x.Email=="vendedor@prueba.invalid").Select(x=>x.Id).SingleAsync();
                var adminId=await usuariosDb.Users.Where(x=>x.Email=="admin@prueba.invalid").Select(x=>x.Id).SingleAsync();
                using var ultimo=await Post("/Usuarios/Actualizar","/Usuarios",new(){["id"]=adminId,["rol"]="Vendedor",["activo"]="true"});
                Verificar(ultimo.StatusCode==HttpStatusCode.Conflict,"No se puede quitar al último administrador");
            }
            using(var desactivar=await Post("/Usuarios/Actualizar","/Usuarios",new(){["id"]=vendedorId,["rol"]="Vendedor",["activo"]="false"}))Verificar(desactivar.StatusCode==HttpStatusCode.Redirect,"Desactivar usuario");
            using(var activar=await Post("/Usuarios/Actualizar","/Usuarios",new(){["id"]=vendedorId,["rol"]="Vendedor",["activo"]="true"}))Verificar(activar.StatusCode==HttpStatusCode.Redirect,"Reactivar usuario");
            using(var recuperar=await Post("/Acceso/Recuperar","/Acceso/Recuperar",new(){["Email"]="admin@prueba.invalid"}))Verificar(recuperar.StatusCode==HttpStatusCode.OK,"Solicitud de recuperación responde sin exponer cuenta ni token");
            var eml=await File.ReadAllTextAsync(Directory.GetFiles(correos,"*.eml").Single());
            var partes=Regex.Split(eml,"\\r?\\n\\r?\\n",RegexOptions.None,TimeSpan.FromSeconds(2));
            var encabezado=partes[0];var cuerpo=string.Join("\n\n",partes.Skip(1));
            if(encabezado.Contains("base64",StringComparison.OrdinalIgnoreCase))cuerpo=System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(Regex.Replace(cuerpo,"\\s", "")));
            else if(encabezado.Contains("quoted-printable",StringComparison.OrdinalIgnoreCase))cuerpo=Regex.Replace(cuerpo.Replace("=\r\n","").Replace("=\n",""),"=([0-9A-Fa-f]{2})",m=>((char)Convert.ToByte(m.Groups[1].Value,16)).ToString());
            var enlace=Regex.Match(cuerpo,"https://[^\\s]+").Value;
            Verificar(Uri.TryCreate(enlace,UriKind.Absolute,out var resetUri),"Correo local contiene enlace de recuperación");
            var parametros=Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(resetUri!.Query);
            var nuevoPassword="Nueva!9"+Convert.ToHexString(RandomNumberGenerator.GetBytes(12));
            using(var reset=await Post("/Acceso/Restablecer",resetUri.PathAndQuery,new(){["Email"]="admin@prueba.invalid",["Token"]=parametros["token"].ToString(),["Password"]=nuevoPassword,["Confirmar"]=nuevoPassword}))Verificar(reset.StatusCode==HttpStatusCode.Redirect,"Restablecimiento de contraseña con token de correo");
            using (var salir = await Post("/Acceso/Salir", "/Acceso/Login", [])) Verificar(salir.StatusCode == HttpStatusCode.Redirect, "Cierre de sesión por POST");
            using (var login = await Post("/Acceso/Login", "/Acceso/Login", new() { ["Email"] = "vendedor@prueba.invalid", ["Password"] = password })) Verificar(login.StatusCode == HttpStatusCode.Redirect, "Login de vendedor");
            using(var salirEquipo=await Post("/Acceso/Salir","/Ventas/Nueva",[])){}
            using(var accesoAdmin=await Post("/Acceso/Login","/Acceso/Login?administrador=true",new(){["Email"]="vendedor@prueba.invalid",["Password"]=password,["administrador"]="true"}))Verificar(accesoAdmin.StatusCode==HttpStatusCode.OK,"Acceso de administrador rechaza cuentas de vendedor");
            using(var loginEquipo=await Post("/Acceso/Login","/Acceso/Login",new(){["Email"]="vendedor@prueba.invalid",["Password"]=password,["returnUrl"]="https://destino-invalido.invalid"}))Verificar(loginEquipo.StatusCode==HttpStatusCode.Redirect&&loginEquipo.Headers.Location?.IsAbsoluteUri!=true,"Acceso normal conserva perfil y rechaza redirecciones externas");
            using (var restringido = await http.GetAsync("/Usuarios")) Verificar(restringido.StatusCode == HttpStatusCode.Redirect && restringido.Headers.Location?.ToString().Contains("Denegado") == true, "Vendedor no accede a usuarios");
            foreach (var ruta in new[] { "/Parametros", "/Ofertas", "/Caja/Detalle/1", "/Compras", "/Administracion", "/Reportes/Excel" })
            {
                using var restringido = await http.GetAsync(ruta);
                Verificar(restringido.StatusCode == HttpStatusCode.Forbidden || (restringido.StatusCode == HttpStatusCode.Redirect && restringido.Headers.Location?.ToString().Contains("Denegado") == true), "Acceso restringido para vendedor: " + ruta);
            }
            await Pagina("/Ventas/Nueva");
        }
        finally
        {
            if (!proceso.HasExited) proceso.Kill(entireProcessTree: true);
            await proceso.WaitForExitAsync();
            await Task.WhenAll(salida, errores);
            var raizTemporal=Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
            if(Directory.Exists(correos)&&Path.GetFullPath(correos).StartsWith(raizTemporal+"RetailPOS_Correos_",StringComparison.OrdinalIgnoreCase))
            {
                foreach(var archivo in Directory.GetFiles(correos,"*.eml"))File.Delete(archivo);
                Directory.Delete(correos);
            }
        }
    }
}



