using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Infrastructure;
namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador")]
public sealed class AdministracionController(TiendaDb db, RespaldosService respaldos) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Respaldos = await db.Respaldos.AsNoTracking().OrderByDescending(x => x.Id).Take(30).ToListAsync();
        return View();
    }
    public async Task<IActionResult> Auditoria(string? entidad, string? usuario, int pagina=1)
    {
        pagina = Math.Max(1,pagina);
        var q = db.Auditoria.AsNoTracking().Where(x => (entidad == null || x.Entidad.Contains(entidad)) && (usuario == null || x.UsuarioId == usuario));
        ViewBag.Total = await q.CountAsync(); ViewBag.Pagina = pagina; ViewBag.Entidad = entidad; ViewBag.Usuario = usuario;
        return View(await q.OrderByDescending(x => x.Id).Skip((pagina-1)*100).Take(100).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Respaldar()
    {
        var id = await respaldos.CrearAsync(db.ActorId); TempData["Ok"] = "Respaldo " + id + " creado y verificado por SQL Server."; return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> ProbarRestauracion(int id)
    {
        await respaldos.ProbarAsync(id); TempData["Ok"] = "Restauración comprobada en una base aislada y eliminada al finalizar. La base de trabajo se conserva."; return RedirectToAction(nameof(Index));
    }
}

