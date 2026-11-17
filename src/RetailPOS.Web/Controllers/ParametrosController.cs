using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Domain;
using RetailPOS.Infrastructure;

namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador")]
public sealed class ParametrosController(TiendaDb db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Marcas = await db.Marcas.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        return View(await db.Tallas.AsNoTracking().OrderBy(x => x.Sistema).ThenBy(x => x.Valor).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Marca([Bind("Nombre")] Marca marca)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(marca.Nombre)) throw new ReglaNegocioException("Indica una marca de hasta 100 caracteres.");
        marca.Nombre = marca.Nombre.Trim(); db.Add(marca); await db.SaveChangesAsync();
        TempData["Ok"] = "Marca creada. Ya puedes seleccionarla al crear un modelo.";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Talla([Bind("Valor,Sistema")] TallaCalzado talla)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(talla.Valor) || talla.Sistema is not ("EU" or "US" or "UK" or "CM")) throw new ReglaNegocioException("Indica talla y sistema EU, US, UK o CM.");
        talla.Valor = talla.Valor.Trim(); db.Add(talla); await db.SaveChangesAsync();
        TempData["Ok"] = "Talla creada."; return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> EstadoMarca(int id, bool activa)
    {
        var marca = await db.Marcas.FindAsync(id); if (marca is null) return NotFound();
        marca.Activa = activa; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> EstadoTalla(int id, bool activa)
    {
        var talla = await db.Tallas.FindAsync(id); if (talla is null) return NotFound();
        talla.Activa = activa; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
}

