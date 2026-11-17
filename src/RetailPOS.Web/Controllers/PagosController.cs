using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Infrastructure;
namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador,Vendedor")]
public sealed class PagosController(TiendaDb db, Operaciones operaciones) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Pagados = await db.Pagos.Where(x => x.DocumentoId != null).GroupBy(x => x.DocumentoId!.Value).ToDictionaryAsync(x => x.Key, x => x.Sum(p => p.Importe));
        return View(await db.Documentos.AsNoTracking().Where(x => x.Tipo == 56).OrderByDescending(x => x.Id).Take(300).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Cobrar(Guid solicitudId, int documentoId, decimal importe, string medio)
    {
        if (!ModelState.IsValid) throw new ReglaNegocioException("Indica un cobro válido.");
        await operaciones.CobrarDebitoAsync(solicitudId, documentoId, importe, medio, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, Authorize(Roles = "Administrador")] public async Task<IActionResult> Cambiar(Guid solicitudId, int detalleId, int nuevaVariante, string medio, string motivo)
    {
        if (!ModelState.IsValid) throw new ReglaNegocioException("Revisa los datos del cambio.");
        var id = await operaciones.CambiarAsync(solicitudId, detalleId, nuevaVariante, medio, motivo, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return RedirectToAction("Detalle", "Ventas", new { id });
    }
}

