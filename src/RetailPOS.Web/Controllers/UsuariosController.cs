using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Infrastructure;
using RetailPOS.Application;
using RetailPOS.Web.Models;

namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador")]
public sealed class UsuariosController(UserManager<UsuarioTienda> users, TiendaDb db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var resultado = new List<UsuarioResumen>();
        foreach (var user in await users.Users.OrderBy(x => x.Email).Take(200).ToListAsync()) resultado.Add(new(user.Id, user.Email ?? "", string.Join(", ", await users.GetRolesAsync(user)),user.Activo));
        ViewBag.Usuarios = resultado;
        return View(new UsuarioForm());
    }
    [HttpPost] public async Task<IActionResult> Crear(UsuarioForm form)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Revisa correo, contraseña (mínimo 12 caracteres) y rol."; return RedirectToAction(nameof(Index)); }
        var user = new UsuarioTienda { Email = form.Email.Trim(), UserName = form.Email.Trim(), LockoutEnabled = true };
        var resultado = await users.CreateAsync(user, form.Password);
        if (!resultado.Succeeded) { TempData["Error"] = string.Join(" ", resultado.Errors.Select(x => x.Description)); return RedirectToAction(nameof(Index)); }
        var asignado = await users.AddToRoleAsync(user, form.Rol);
        if (!asignado.Succeeded) { await users.DeleteAsync(user); TempData["Error"] = "No se pudo asignar el rol."; }
        else
        {
            db.Auditoria.Add(new(){UsuarioId=db.ActorId,Entidad="Usuario",Registro=user.Id,Accion="CrearCuenta",Campos="Rol="+form.Rol});
            await db.SaveChangesAsync();TempData["Ok"] = "Usuario creado.";
        }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Actualizar(string id, string rol, bool activo)
    {
        if (rol is not ("Administrador" or "Vendedor" or "Bodega")) throw new ReglaNegocioException("Rol inválido.");
        await using var tx = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=N'Usuarios.Administradores',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000; IF @r<0 THROW 51000,'Reintenta la operación.',1;");
        var user = await users.FindByIdAsync(id) ?? throw new ReglaNegocioException("Usuario inexistente.");
        var roles = await users.GetRolesAsync(user);
        if (user.Activo && roles.Contains("Administrador") && (!activo || rol != "Administrador") && (await users.GetUsersInRoleAsync("Administrador")).Count(x => x.Activo) <= 1) throw new ReglaNegocioException("Debe quedar al menos un administrador activo.");
        user.Activo = activo;
        var resultado = await users.UpdateAsync(user); if (!resultado.Succeeded) throw new ReglaNegocioException("No se pudo actualizar la cuenta.");
        if (!(await users.RemoveFromRolesAsync(user,roles)).Succeeded || !(await users.AddToRoleAsync(user,rol)).Succeeded || !(await users.UpdateSecurityStampAsync(user)).Succeeded) throw new ReglaNegocioException("No se pudo actualizar el rol.");
        db.Auditoria.Add(new() { UsuarioId = db.ActorId, Entidad = "Usuario", Registro = id, Accion = "EstadoYRol", Campos = $"Activo={activo};Rol={rol}" });
        await db.SaveChangesAsync(); await tx.CommitAsync(); return RedirectToAction(nameof(Index));
    }
}
public sealed record UsuarioResumen(string Id, string Email, string Roles, bool Activo);


