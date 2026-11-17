using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Infrastructure;
using RetailPOS.Web.Models;

namespace RetailPOS.Web.Controllers;

[AllowAnonymous, EnableRateLimiting("acceso")]
public sealed class AccesoController(TiendaDb db, UserManager<UsuarioTienda> users, RoleManager<IdentityRole> roles, SignInManager<UsuarioTienda> signIn, IWebHostEnvironment env, IConfiguration config, CorreoRecuperacion correo, ILogger<AccesoController> logger) : Controller
{
    private bool PuedeConfigurar => env.IsDevelopment() && config.GetValue<bool>("Bootstrap:AllowLocalSetup") && HttpContext.Connection.RemoteIpAddress is { } ip && IPAddress.IsLoopback(ip);
    [HttpGet] public async Task<IActionResult> Login(bool administrador=false,string? returnUrl=null)
    {
        ViewBag.PuedeConfigurar = PuedeConfigurar && !await db.Users.AnyAsync();
        ViewBag.AccesoAdministrador=administrador;
        ViewBag.ReturnUrl=Url.IsLocalUrl(returnUrl)?returnUrl:null;
        return View(new AccesoForm());
    }
    [HttpPost] public async Task<IActionResult> Login(AccesoForm form,bool administrador=false,string? returnUrl=null)
    {
        if (ModelState.IsValid)
        {
            var cuenta = await users.FindByEmailAsync(form.Email.Trim());
            var autorizado=cuenta is {Activo:true}&&(!administrador||await users.IsInRoleAsync(cuenta,"Administrador"));
            var result = autorizado ? await signIn.PasswordSignInAsync(cuenta!, form.Password, false, lockoutOnFailure: true) : Microsoft.AspNetCore.Identity.SignInResult.Failed;
            if (result.Succeeded) return Url.IsLocalUrl(returnUrl)?LocalRedirect(returnUrl!):RedirectToAction("Index", "Inicio");
            ModelState.AddModelError("", "No se pudo iniciar sesión. Revisa tus credenciales o inténtalo más tarde.");
        }
        ViewBag.PuedeConfigurar = PuedeConfigurar && !await db.Users.AnyAsync();
        ViewBag.AccesoAdministrador=administrador;
        ViewBag.ReturnUrl=Url.IsLocalUrl(returnUrl)?returnUrl:null;
        return View(form);
    }
    [HttpGet] public async Task<IActionResult> Administradores()
    {
        if(!await db.Users.AnyAsync())return PuedeConfigurar?RedirectToAction(nameof(Configurar)):RedirectToAction(nameof(Login));
        if(User.Identity?.IsAuthenticated!=true)return RedirectToAction(nameof(Login),new{administrador=true,returnUrl="/Usuarios"});
        return User.IsInRole("Administrador")?RedirectToAction("Index","Usuarios"):Forbid();
    }
    [HttpGet] public async Task<IActionResult> Registro()
    {
        if(!await db.Users.AnyAsync())
        {
            if(PuedeConfigurar)return RedirectToAction(nameof(Configurar));
            TempData["Error"]="Primero un administrador debe configurar la tienda.";return RedirectToAction(nameof(Login));
        }
        return View(new RegistroForm());
    }
    [HttpPost] public async Task<IActionResult> Registro(RegistroForm form)
    {
        if(!ModelState.IsValid)return View(form);
        await using var tx=await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=N'RetailPOS.Bootstrap',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000; IF @r<0 THROW 51000,'Reintenta el registro.',1;");
        if(!await db.Users.AnyAsync()||!await roles.RoleExistsAsync("Vendedor"))return RedirectToAction(nameof(Registro));
        var user=new UsuarioTienda{UserName=form.Email.Trim(),Email=form.Email.Trim(),Activo=false,LockoutEnabled=true};
        var resultado=await users.CreateAsync(user,form.Password);
        if(!resultado.Succeeded){foreach(var e in resultado.Errors)ModelState.AddModelError("",e.Description);return View(form);}
        if(!(await users.AddToRoleAsync(user,"Vendedor")).Succeeded)throw new InvalidOperationException("No se pudo preparar la cuenta.");
        db.Auditoria.Add(new(){UsuarioId=user.Id,Entidad="Usuario",Registro=user.Id,Accion="RegistroPendiente",Campos="Cuenta pendiente de activación por administrador"});
        await db.SaveChangesAsync();await tx.CommitAsync();
        TempData["Ok"]="Tu cuenta fue creada. Un administrador debe activarla antes de que puedas ingresar.";
        return RedirectToAction(nameof(Login));
    }
    [HttpGet] public async Task<IActionResult> Configurar()
    {
        if (!PuedeConfigurar || await db.Users.AnyAsync()) return NotFound();
        return View(new AccesoForm());
    }
    [HttpPost] public async Task<IActionResult> Configurar(AccesoForm form)
    {
        if (!PuedeConfigurar) return NotFound();
        if (!ModelState.IsValid) return View(form);
        await using var tx = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=N'RetailPOS.Bootstrap', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000; IF @r<0 THROW 51000, 'No se obtuvo bloqueo.',1;");
        if (await db.Users.AnyAsync()) return NotFound();
        foreach (var rol in new[] { "Administrador", "Vendedor", "Bodega" })
        {
            if (!await roles.RoleExistsAsync(rol))
            {
                var creado = await roles.CreateAsync(new IdentityRole(rol));
                if (!creado.Succeeded) throw new InvalidOperationException("No se pudo crear el rol.");
            }
        }
        var user = new UsuarioTienda { UserName = form.Email.Trim(), Email = form.Email.Trim(), LockoutEnabled = true };
        var result = await users.CreateAsync(user, form.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(form);
        }
        var asignado = await users.AddToRoleAsync(user, "Administrador");
        if (!asignado.Succeeded) throw new InvalidOperationException("No se pudo asignar el rol.");
        await tx.CommitAsync();
        return RedirectToAction(nameof(Login));
    }
    [HttpPost] public async Task<IActionResult> Salir() { await signIn.SignOutAsync(); return RedirectToAction(nameof(Login)); }
    [HttpGet] public IActionResult Recuperar() => View(new RecuperacionForm());
    [HttpPost] public async Task<IActionResult> Recuperar(RecuperacionForm form)
    {
        if (!ModelState.IsValid) return View(form);
        var user = await users.FindByEmailAsync(form.Email.Trim());
        if (user is { Activo: true })
        {
            var token = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(await users.GeneratePasswordResetTokenAsync(user)));
            var baseUrl = config["Correo:UrlPublica"] ?? "https://localhost:7243";
            if (!Uri.TryCreate(baseUrl,UriKind.Absolute,out var url) || url.Scheme != "https") throw new InvalidOperationException("La URL de recuperación debe ser HTTPS.");
            var enlace = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(baseUrl.TrimEnd('/') + "/Acceso/Restablecer", new Dictionary<string,string?> { ["email"] = user.Email, ["token"] = token });
            try { await correo.EnviarAsync(user.Email!, enlace); }
            catch (Exception ex) when (ex is System.Net.Mail.SmtpException or InvalidOperationException or IOException) { logger.LogWarning("No se pudo entregar un correo de recuperación. Revisa la configuración del servicio."); }
        }
        ViewBag.Enviado = true; return View(new RecuperacionForm());
    }
    [HttpGet] public IActionResult Restablecer(string email, string token) => View(new RestablecerForm { Email = email, Token = token });
    [HttpPost] public async Task<IActionResult> Restablecer(RestablecerForm form)
    {
        if (!ModelState.IsValid) return View(form);
        var user = await users.FindByEmailAsync(form.Email);
        IdentityResult resultado = IdentityResult.Failed();
        try
        {
            var token = System.Text.Encoding.UTF8.GetString(Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(form.Token));
            if (user is { Activo: true }) resultado = await users.ResetPasswordAsync(user, token, form.Password);
        }
        catch (FormatException) { }
        if (!resultado.Succeeded) { ModelState.AddModelError("", "Enlace inválido o vencido, o contraseña que no cumple los requisitos."); return View(form); }
        db.Auditoria.Add(new() { UsuarioId = user!.Id, Entidad = "Usuario", Registro = user.Id, Accion = "RecuperarAcceso", Campos = "Contraseña restablecida (sin registrar secretos)" });
        await db.SaveChangesAsync(); TempData["Ok"] = "Contraseña actualizada. Inicia sesión."; return RedirectToAction(nameof(Login));
    }
    public IActionResult Denegado() { Response.StatusCode = 403; return View("OperacionError", "Tu perfil no tiene permiso para esta operación."); }
    [IgnoreAntiforgeryToken] public IActionResult Error() { Response.StatusCode = 500; return View("OperacionError", "No pudimos completar la solicitud. Revisa el servicio y vuelve a intentarlo."); }
}


