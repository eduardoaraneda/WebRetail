using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Infrastructure;
namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador,Bodega")]
public sealed class InventarioControlController(TiendaDb db, Operaciones operaciones) : Controller
{
    private string Usuario => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index()
    {
        ViewBag.Bodegas = await db.Bodegas.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.Variantes = await db.Variantes.AsNoTracking().OrderBy(x => x.Sku).ToListAsync();
        ViewBag.Revisiones = await db.RevisionesStock.AsNoTracking().Include(x => x.Variante).Include(x => x.Bodega).OrderByDescending(x => x.Id).Take(200).ToListAsync();
        return View(await db.Existencias.AsNoTracking().Include(x => x.Variante).Include(x => x.Bodega).OrderBy(x => x.Cantidad > x.StockMinimo).ThenBy(x => x.Variante.Sku).Take(1000).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Operar(Guid solicitudId, string tipo, int varianteId, int bodegaId, int? destinoId, int cantidad, string motivo)
    {
        if (!ModelState.IsValid) throw new ReglaNegocioException("Revisa los campos de inventario.");
        await operaciones.OperarStockAsync(solicitudId, tipo, varianteId, bodegaId, destinoId, cantidad, motivo, Usuario);
        TempData["Ok"] = "Operación de inventario registrada."; return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Resolver(int id, string estado, string motivo)
    {
        await operaciones.ResolverRevisionAsync(id, estado, motivo, Usuario); return RedirectToAction(nameof(Index));
    }
    [HttpPost, Authorize(Roles = "Administrador")] public async Task<IActionResult> Parametros(int varianteId, int bodegaId, int minimo, decimal? costo, string motivo)
    {
        if (!ModelState.IsValid || minimo is < 0 or > 100000 || costo is < 0 or > 999999999 || (costo.HasValue && decimal.Round(costo.Value,2) != costo) || string.IsNullOrWhiteSpace(motivo)) throw new ReglaNegocioException("Indica mínimo, costo válido (o vacío si se desconoce) y motivo.");
        var stock = await db.Existencias.SingleOrDefaultAsync(x => x.VarianteId == varianteId && x.BodegaId == bodegaId) ?? throw new ReglaNegocioException("Ingresa existencias antes de configurar esta combinación.");
        stock.StockMinimo = minimo; stock.CostoPromedio = costo;
        db.Auditoria.Add(new() { UsuarioId = Usuario, Entidad = "Existencia", Registro = $"{varianteId}/{bodegaId}", Accion = "AjusteCosto", Campos = motivo[..Math.Min(2000,motivo.Length)] });
        await db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
}

