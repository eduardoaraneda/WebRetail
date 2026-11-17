using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Infrastructure;

namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador,Vendedor")]
public sealed class DocumentosController(TiendaDb db, IOperaciones operaciones) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Documentos.AsNoTracking().OrderByDescending(x => x.Id).Take(200).ToListAsync());
    public async Task<IActionResult> Detalle(int id)
    {
        var doc = await db.Documentos.AsNoTracking().Include(x => x.Detalles).Include(x => x.Venta).ThenInclude(x => x.Cliente).SingleOrDefaultAsync(x => x.Id == id);
        return doc is null ? NotFound() : View(doc);
    }
    [HttpPost] public async Task<IActionResult> Guia(int ventaId, int referenciaId, string motivo)
    {
        if (!ModelState.IsValid) throw new ReglaNegocioException("Revisa la referencia y el motivo.");
        var id = await operaciones.CrearDocumentoAsync(ventaId, 52, referenciaId, motivo, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return RedirectToAction(nameof(Detalle), new { id });
    }
    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Devolver(Guid solicitudId, int detalleVentaId, int cantidad, bool reingresarStock, string motivo)
    {
        if (!ModelState.IsValid) throw new ReglaNegocioException("Revisa los datos de la devolución.");
        var id = await operaciones.DevolverAsync(solicitudId, detalleVentaId, cantidad, reingresarStock, motivo, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return RedirectToAction(nameof(Detalle), new { id });
    }
    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Debito(Guid solicitudId, int referenciaId, decimal importe, string motivo)
    {
        if (!ModelState.IsValid) throw new ReglaNegocioException("Revisa el importe y la referencia.");
        var id = await operaciones.DebitarAsync(solicitudId, referenciaId, importe, motivo, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return RedirectToAction(nameof(Detalle), new { id });
    }
}

