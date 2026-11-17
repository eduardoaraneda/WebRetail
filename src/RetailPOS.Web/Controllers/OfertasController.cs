using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Domain;
using RetailPOS.Infrastructure;

namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador")]
public sealed class OfertasController(TiendaDb db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Marcas = await db.Marcas.AsNoTracking().Where(x => x.Activa).OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.Tallas = await db.Tallas.AsNoTracking().Where(x => x.Activa).OrderBy(x => x.Sistema).ThenBy(x => x.Valor).ToListAsync();
        ViewBag.Variantes = await db.Variantes.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Sku).ToListAsync();
        return View(await db.Ofertas.AsNoTracking().Include(x => x.Marca).Include(x => x.Talla).Include(x => x.Variante).OrderByDescending(x => x.Id).Take(500).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Crear([Bind("Nombre,MarcaId,TallaId,VarianteId,Tipo,Valor,Inicio,Fin")] Oferta oferta)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(oferta.Nombre) || oferta.Inicio.Year < 2000 || oferta.Fin < oferta.Inicio || oferta.Valor <= 0 || oferta.Valor > 999999999 || oferta.Tipo is not ("Porcentaje" or "Monto") || (oferta.Tipo == "Porcentaje" && oferta.Valor > 100) || (oferta.Tipo == "Monto" && decimal.Truncate(oferta.Valor) != oferta.Valor) || decimal.Round(oferta.Valor, 2) != oferta.Valor)
            throw new ReglaNegocioException("Revisa nombre, fechas y descuento. Porcentaje: mayor a 0 y hasta 100, con hasta dos decimales. Monto: pesos enteros positivos.");
        if (oferta.MarcaId is null && oferta.TallaId is null && oferta.VarianteId is null) throw new ReglaNegocioException("Selecciona al menos una marca, talla o SKU.");
        if (oferta.MarcaId.HasValue && !await db.Marcas.AnyAsync(x => x.Id == oferta.MarcaId && x.Activa) || oferta.TallaId.HasValue && !await db.Tallas.AnyAsync(x => x.Id == oferta.TallaId && x.Activa) || oferta.VarianteId.HasValue && !await db.Variantes.AnyAsync(x => x.Id == oferta.VarianteId && x.Activo))
            throw new ReglaNegocioException("Selecciona filtros existentes y activos.");
        var coincidencias = db.Variantes.Where(x => x.Activo && x.Producto.Activo);
        if (oferta.MarcaId.HasValue) { var marca = await db.Marcas.SingleAsync(x => x.Id == oferta.MarcaId); coincidencias = coincidencias.Where(x => x.Producto.Marca == marca.Nombre); }
        if (oferta.TallaId.HasValue) { var talla = await db.Tallas.SingleAsync(x => x.Id == oferta.TallaId); coincidencias = coincidencias.Where(x => x.Talla == talla.Valor && x.SistemaTalla == talla.Sistema); }
        if (oferta.VarianteId.HasValue) coincidencias = coincidencias.Where(x => x.Id == oferta.VarianteId);
        if (!await coincidencias.AnyAsync()) throw new ReglaNegocioException("No hay variantes activas que coincidan con todos los filtros seleccionados.");
        oferta.Nombre = oferta.Nombre.Trim(); oferta.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        db.Add(oferta); await db.SaveChangesAsync(); TempData["Ok"] = "Oferta creada. Se aplicará automáticamente dentro de su vigencia.";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Estado(int id, bool activa)
    {
        var oferta = await db.Ofertas.FindAsync(id); if (oferta is null) return NotFound();
        oferta.Activa = activa; await db.SaveChangesAsync();
        TempData["Ok"] = "Estado actualizado. Las ventas previas mantienen su descuento original.";
        return RedirectToAction(nameof(Index));
    }
}

