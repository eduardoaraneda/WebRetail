using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Domain;
using RetailPOS.Infrastructure;

namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador,Bodega")]
public sealed class InventarioController(TiendaDb db, IOperaciones operaciones) : Controller
{
    public async Task<IActionResult> Index(string? q,int? bodegaId,bool alertas=false,int pagina=1)
    {
        ViewBag.Bodegas = await db.Bodegas.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.Variantes = await db.Variantes.AsNoTracking().Include(x => x.Producto).Where(x => x.Activo).OrderBy(x => x.Sku).ToListAsync();
        ViewBag.Movimientos = await db.Movimientos.AsNoTracking().Include(x => x.Variante).Include(x => x.Bodega).OrderByDescending(x => x.Id).Take(100).ToListAsync();
        var consulta=db.Existencias.AsNoTracking().Include(x=>x.Variante).ThenInclude(x=>x.Producto).Include(x=>x.Bodega).Where(x=>(q==null||x.Variante.Sku.Contains(q)||x.Variante.Producto.Modelo.Contains(q))&&(!bodegaId.HasValue||x.BodegaId==bodegaId)&&(!alertas||x.Cantidad<=x.StockMinimo));
        pagina=Math.Max(1,pagina); ViewBag.Pagina=pagina;ViewBag.Total=await consulta.CountAsync();ViewBag.Q=q;ViewBag.BodegaId=bodegaId;ViewBag.Alertas=alertas;
        return View(await consulta.OrderBy(x=>x.Bodega.Nombre).ThenBy(x=>x.Variante.Sku).Skip((pagina-1)*50).Take(50).ToListAsync());
    }
    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Bodega([Bind("Nombre,Tienda")] Bodega bodega)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(bodega.Nombre)) throw new ReglaNegocioException("Indica un nombre de bodega de hasta 120 caracteres.");
        bodega.Nombre = bodega.Nombre.Trim(); db.Add(bodega); await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    [HttpPost,Authorize(Roles="Administrador")] public async Task<IActionResult> EditarBodega(int id,string nombre,string tienda)
    {
        if(string.IsNullOrWhiteSpace(nombre)||nombre.Length>120||string.IsNullOrWhiteSpace(tienda)||tienda.Length>100) throw new ReglaNegocioException("Indica bodega y tienda válidas.");
        var b=await db.Bodegas.FindAsync(id);if(b is null)return NotFound();b.Nombre=nombre.Trim();b.Tienda=tienda.Trim();await db.SaveChangesAsync();return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Movimiento(int varianteId, int bodegaId, int cantidad, string motivo)
    {
        if (!ModelState.IsValid) throw new ReglaNegocioException("Revisa los datos del movimiento.");
        await operaciones.MoverStockAsync(varianteId, bodegaId, cantidad, motivo, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        TempData["Ok"] = "Movimiento registrado."; return RedirectToAction(nameof(Index));
    }
}

