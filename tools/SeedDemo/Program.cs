using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetailPOS.Domain;
using RetailPOS.Infrastructure;

if(!args.Contains("--confirmar-demo"))throw new InvalidOperationException("Usar --confirmar-demo para crear datos ficticios en la base local.");
var configuracion=JsonDocument.Parse(await File.ReadAllTextAsync("src/RetailPOS.Web/appsettings.Development.json"));
var conexion=Environment.GetEnvironmentVariable("ConnectionStrings__Tienda")??configuracion.RootElement.GetProperty("ConnectionStrings").GetProperty("Tienda").GetString()!;
var servicios=new ServiceCollection();servicios.AddLogging();servicios.AddDbContext<TiendaDb>(o=>o.UseSqlServer(conexion));
servicios.AddIdentityCore<UsuarioTienda>(o=>{o.Password.RequiredLength=12;o.User.RequireUniqueEmail=true;}).AddRoles<IdentityRole>().AddEntityFrameworkStores<TiendaDb>();
await using var provider=servicios.BuildServiceProvider();await using var scope=provider.CreateAsyncScope();
var db=scope.ServiceProvider.GetRequiredService<TiendaDb>();
if(db.Database.GetDbConnection().Database is not ("ZapatillasPOS" or "RetailPOS"))throw new InvalidOperationException("Base de destino no autorizada para datos demo.");
var usuarios=scope.ServiceProvider.GetRequiredService<UserManager<UsuarioTienda>>();var roles=scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
await using var tx=await db.Database.BeginTransactionAsync();
await db.Database.ExecuteSqlRawAsync("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=N'RetailPOS.SeedDemo',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000; IF @r<0 THROW 51000,'No se obtuvo bloqueo.',1;");
var email="admin.demo@retail.local";
if(await usuarios.FindByEmailAsync(email) is not null)email="admin.demo."+Guid.NewGuid().ToString("N")[..8]+"@retail.local";
var password="Retail!7"+Convert.ToHexString(RandomNumberGenerator.GetBytes(10));
foreach(var nombre in new[]{"Administrador","Vendedor","Bodega"})
{
    if(!await roles.RoleExistsAsync(nombre)&&!(await roles.CreateAsync(new IdentityRole(nombre))).Succeeded)throw new Exception("No se pudo crear un rol.");
}
var administrador=new UsuarioTienda{UserName=email,Email=email,Activo=true,LockoutEnabled=true};
var resultado=await usuarios.CreateAsync(administrador,password);
if(!resultado.Succeeded)throw new Exception(string.Join(" ",resultado.Errors.Select(x=>x.Description)));
if(!(await usuarios.AddToRoleAsync(administrador,"Administrador")).Succeeded)throw new Exception("No se pudo asignar el rol.");
db.ActorId=administrador.Id;
var bodega=await db.Bodegas.SingleOrDefaultAsync(x=>x.Nombre=="Bodega Demo");
if(bodega is null){bodega=new(){Nombre="Bodega Demo",Tienda="Tienda Demo"};db.Add(bodega);}
foreach(var talla in new[]{"38","39","40","41","42"})if(!await db.Tallas.AnyAsync(x=>x.Valor==talla&&x.Sistema=="EU"))db.Add(new TallaCalzado{Valor=talla,Sistema="EU"});
foreach(var marca in new[]{"Demo Urban","Demo Sport","Demo Andes"})if(!await db.Marcas.AnyAsync(x=>x.Nombre==marca))db.Add(new Marca{Nombre=marca});
await db.SaveChangesAsync();
var modelos=new[]{("Demo Urban","City Street","CITY",44990m),("Demo Urban","Daily Comfort","DAILY",39990m),("Demo Sport","Runner Flex","RUN",59990m),("Demo Sport","Training Pro","TRAIN",69990m),("Demo Andes","Trail Explorer","TRAIL",79990m),("Demo Andes","Outdoor Lite","OUT",49990m)};
var nuevosModelos=0;var nuevasVariantes=0;var unidades=0;
foreach(var (marca,modelo,prefijo,precio) in modelos)
{
    var producto=await db.Productos.SingleOrDefaultAsync(x=>x.Marca==marca&&x.Modelo==modelo);
    if(producto is null){producto=new(){Marca=marca,Modelo=modelo};db.Add(producto);await db.SaveChangesAsync();nuevosModelos++;}
    foreach(var talla in new[]{"38","39","40","41","42"})foreach(var color in new[]{"Negro","Blanco"})
    {
        var sku=$"DEMO-{prefijo}-{talla}-{color[..3].ToUpperInvariant()}";
        if(await db.Variantes.AnyAsync(x=>x.Sku==sku))continue;
        var variante=new Variante{ProductoId=producto.Id,Sku=sku,Color=color,Talla=talla,SistemaTalla="EU",Precio=precio};db.Add(variante);await db.SaveChangesAsync();
        db.Add(new Existencia{VarianteId=variante.Id,BodegaId=bodega.Id,Cantidad=8,StockMinimo=3,CostoPromedio=decimal.Round(precio*.55m,0)});
        db.Add(new Movimiento{VarianteId=variante.Id,BodegaId=bodega.Id,Cantidad=8,Motivo="Carga inicial ficticia de demostración",UsuarioId=administrador.Id});
        nuevasVariantes++;unidades+=8;
    }
}
db.Auditoria.Add(new(){UsuarioId=administrador.Id,Entidad="Usuario",Registro=administrador.Id,Accion="CrearAdministradorDemo",Campos="Cuenta solicitada por el propietario; contraseña generada, no registrada"});
await db.SaveChangesAsync();
if(!await usuarios.CheckPasswordAsync(administrador,password)||!await usuarios.IsInRoleAsync(administrador,"Administrador"))throw new Exception("No se pudo verificar el acceso creado.");
await tx.CommitAsync();
Console.WriteLine(JsonSerializer.Serialize(new{Correo=email,Password=password,ModelosAgregados=nuevosModelos,VariantesAgregadas=nuevasVariantes,UnidadesAgregadas=unidades,Bodega=bodega.Nombre,AdministradorActivo=administrador.Activo}));
