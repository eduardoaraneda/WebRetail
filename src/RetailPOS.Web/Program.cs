using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Infrastructure;
using RetailPOS.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TiendaDb>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Tienda")
    ?? throw new InvalidOperationException("Falta la conexión Tienda.")));
builder.Services.AddIdentity<UsuarioTienda, IdentityRole>(o =>
{
    o.User.RequireUniqueEmail = true;
    o.Password.RequiredLength = 12;
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
}).AddEntityFrameworkStores<TiendaDb>().AddDefaultTokenProviders().AddErrorDescriber<IdentidadEspanol>();
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Acceso/Login";
    o.AccessDeniedPath = "/Acceso/Denegado";
    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    o.SlidingExpiration = false;
    o.Events.OnValidatePrincipal = async context =>
    {
        var emitida=context.Properties.IssuedUtc; var vence=context.Properties.ExpiresUtc;
        await SecurityStampValidator.ValidatePrincipalAsync(context);
        context.Properties.IssuedUtc=emitida;context.Properties.ExpiresUtc=vence;context.ShouldRenew=false;
        if (context.Principal is null) return;
        var users = context.HttpContext.RequestServices.GetRequiredService<UserManager<UsuarioTienda>>();
        var user = await users.GetUserAsync(context.Principal);
        if (user is null || !user.Activo) { context.RejectPrincipal(); await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme); }
    };
});
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.Zero);
builder.Services.AddAuthorization(o => o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddScoped<IOperaciones, Operaciones>();
builder.Services.AddScoped<Operaciones>();
builder.Services.AddScoped<CorreoRecuperacion>();
builder.Services.AddScoped<RespaldosService>();
builder.Services.AddControllersWithViews(o =>
{
    o.ModelBinderProviders.Insert(0,new ImporteHtmlProvider());
    o.ModelMetadataDetailsProviders.Add(new ValidacionEspanol());
    var mensajes=o.ModelBindingMessageProvider;
    mensajes.SetValueMustNotBeNullAccessor(_=>"Este campo es obligatorio.");
    mensajes.SetAttemptedValueIsInvalidAccessor((_,campo)=>$"El valor ingresado en {campo} no es válido.");
    mensajes.SetNonPropertyAttemptedValueIsInvalidAccessor(_=>"El valor ingresado no es válido.");
    mensajes.SetUnknownValueIsInvalidAccessor(campo=>$"Revisa el valor de {campo}.");
    mensajes.SetNonPropertyUnknownValueIsInvalidAccessor(()=>"El valor ingresado no es válido.");
    mensajes.SetValueIsInvalidAccessor(_=>"El valor ingresado no es válido.");
    mensajes.SetValueMustBeANumberAccessor(campo=>$"El campo {campo} debe ser un número.");
    mensajes.SetNonPropertyValueMustBeANumberAccessor(()=>"Debes ingresar un número.");
    mensajes.SetMissingBindRequiredValueAccessor(campo=>$"El campo {campo} es obligatorio.");
    mensajes.SetMissingKeyOrValueAccessor(()=>"Completa los datos solicitados.");
    mensajes.SetMissingRequestBodyRequiredValueAccessor(()=>"No se recibieron los datos del formulario.");
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    o.Filters.Add<ErroresOperacion>();
});
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = 429;
    o.OnRejected=async (context,ct)=>{context.HttpContext.Response.ContentType="text/plain; charset=utf-8";await context.HttpContext.Response.WriteAsync("Has realizado demasiados intentos. Espera cinco minutos antes de volver a intentarlo.",ct);};
    o.AddPolicy("acceso", ctx => RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString() ?? "local", _ => new FixedWindowRateLimiterOptions
    { PermitLimit = builder.Configuration.GetValue("Acceso:LimiteIntentos",20), Window = TimeSpan.FromMinutes(5), QueueLimit = 0 }));
});
var app = builder.Build();
if(args.Contains("--respaldo-previo"))
{
    using var scope=app.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<TiendaDb>();
    if(db.Database.GetDbConnection().Database is not ("RetailPOS" or "ZapatillasPOS"))throw new InvalidOperationException("Comando limitado a la base de Punto de Venta Retail.");
    Console.WriteLine("Respaldo previo verificado: "+await scope.ServiceProvider.GetRequiredService<RespaldosService>().CrearArchivoAsync());return;
}
if(args.Contains("--respaldar") || args.Contains("--probar-respaldo"))
{
    using var scope=app.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<TiendaDb>();
    if(db.Database.GetDbConnection().Database is not ("RetailPOS" or "ZapatillasPOS"))throw new InvalidOperationException("Comando limitado a la base de Punto de Venta Retail.");
    var servicio=scope.ServiceProvider.GetRequiredService<RespaldosService>();db.ActorId="operador-local";
    if(args.Contains("--respaldar")){var id=await servicio.CrearAsync("operador-local");Console.WriteLine("Respaldo creado y verificado. ID: "+id);}
    else{var indice=Array.IndexOf(args,"--probar-respaldo");if(indice+1>=args.Length||!int.TryParse(args[indice+1],out var id))throw new InvalidOperationException("Indica el ID de respaldo.");await servicio.ProbarAsync(id);Console.WriteLine("Restauración aislada comprobada.");}
    return;
}
if (args.Contains("--crear-base"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TiendaDb>();
    if (db.Database.GetDbConnection().Database is not ("RetailPOS" or "ZapatillasPOS")) throw new InvalidOperationException("Este comando solo prepara la base de Punto de Venta Retail.");
    await db.Database.MigrateAsync();
    Console.WriteLine("Base de Punto de Venta Retail preparada.");
    return;
}
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseExceptionHandler("/Acceso/Error");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    context.RequestServices.GetRequiredService<TiendaDb>().ActorId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonimo";
    await next();
});
app.MapGet("/salud", async (TiendaDb db) => await db.Database.CanConnectAsync() ? Results.Ok(new { estado = "Disponible" }) : Results.StatusCode(503)).AllowAnonymous();
app.MapControllerRoute("default", "{controller=Inicio}/{action=Index}/{id?}");
app.Run();


