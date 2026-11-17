using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Domain;
using RetailPOS.Infrastructure;
using RetailPOS.Web.Models;

namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador,Bodega")]
public sealed class CatalogoController(TiendaDb db) : Controller
{
    public async Task<IActionResult> Index(string? q,int pagina=1)
    {
        ViewBag.Productos = await db.Productos.AsNoTracking().OrderBy(x => x.Marca).ThenBy(x => x.Modelo).ToListAsync();
        ViewBag.Marcas = await db.Marcas.AsNoTracking().Where(x => x.Activa).OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.Tallas = await db.Tallas.AsNoTracking().Where(x => x.Activa).OrderBy(x => x.Sistema).ThenBy(x => x.Valor).ToListAsync();
        var consulta=db.Variantes.AsNoTracking().Include(x=>x.Producto).Where(x=>q==null || x.Sku.Contains(q) || x.Producto.Modelo.Contains(q) || x.Producto.Marca.Contains(q));
        pagina=Math.Max(1,pagina); ViewBag.Q=q; ViewBag.Pagina=pagina; ViewBag.Total=await consulta.CountAsync();
        return View(await consulta.OrderBy(x=>x.Sku).Skip((pagina-1)*50).Take(50).ToListAsync());
    }
    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Producto([Bind("Marca,Modelo")] Producto producto)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(producto.Marca) || string.IsNullOrWhiteSpace(producto.Modelo)) throw new ReglaNegocioException("Completa marca y modelo (máximo 100 y 150 caracteres).");
        producto.Marca = producto.Marca.Trim(); producto.Modelo = producto.Modelo.Trim();
        if (!await db.Marcas.AnyAsync(x => x.Nombre == producto.Marca && x.Activa)) throw new ReglaNegocioException("Crea o activa la marca antes de seleccionar el modelo.");
        db.Add(producto); await db.SaveChangesAsync();
        TempData["Ok"] = "Modelo creado. Agrega sus tallas y colores.";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Variante(VarianteForm form)
    {
        if (!ModelState.IsValid || decimal.Truncate(form.Precio) != form.Precio) throw new ReglaNegocioException("Revisa los campos de la variante. El precio debe ser entero en pesos.");
        if (!await db.Productos.AnyAsync(x => x.Id == form.ProductoId && x.Activo)) throw new ReglaNegocioException("El modelo no existe o está inactivo.");
        var talla = await db.Tallas.SingleOrDefaultAsync(x => x.Id == form.TallaId && x.Activa) ?? throw new ReglaNegocioException("Selecciona una talla activa del catálogo.");
        db.Add(new Variante { ProductoId = form.ProductoId, Sku = form.Sku.Trim().ToUpperInvariant(), CodigoBarras = string.IsNullOrWhiteSpace(form.CodigoBarras) ? null : form.CodigoBarras.Trim(), Color = form.Color.Trim(), Talla = talla.Valor, SistemaTalla = talla.Sistema, Precio = form.Precio });
        await db.SaveChangesAsync(); TempData["Ok"] = "Variante creada.";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, decimal precio, bool activo)
    {
        if (precio is < 1 or > 999999999 || decimal.Truncate(precio) != precio) throw new ReglaNegocioException("El precio debe ser entero y mayor a cero.");
        var variante = await db.Variantes.FindAsync(id); if (variante is null) return NotFound();
        variante.Precio = precio; variante.Activo = activo;
        await db.SaveChangesAsync(); TempData["Ok"] = "Variante actualizada. Las ventas anteriores conservan su precio.";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, Authorize(Roles="Administrador")] public async Task<IActionResult> EditarModelo(int id,string marca,string modelo,bool activo)
    {
        if(string.IsNullOrWhiteSpace(modelo)||modelo.Length>150||!await db.Marcas.AnyAsync(x=>x.Nombre==marca && x.Activa)) throw new ReglaNegocioException("Selecciona marca y modelo válidos.");
        var p=await db.Productos.FindAsync(id); if(p is null) return NotFound();
        p.Marca=marca; p.Modelo=modelo.Trim(); p.Activo=activo; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
    [HttpPost,Authorize(Roles="Administrador")] public async Task<IActionResult> EditarVariante(int id,int tallaId,string color,string? codigoBarras)
    {
        if(!ModelState.IsValid||string.IsNullOrWhiteSpace(color)||color.Length>60||codigoBarras?.Length>60)throw new ReglaNegocioException("Revisa color y código de barras.");
        var talla=await db.Tallas.SingleOrDefaultAsync(x=>x.Id==tallaId&&x.Activa)??throw new ReglaNegocioException("Talla no disponible.");
        var variante=await db.Variantes.FindAsync(id);if(variante is null)return NotFound();
        variante.Talla=talla.Valor;variante.SistemaTalla=talla.Sistema;variante.Color=color.Trim();variante.CodigoBarras=string.IsNullOrWhiteSpace(codigoBarras)?null:codigoBarras.Trim();await db.SaveChangesAsync();return RedirectToAction(nameof(Index));
    }
}

