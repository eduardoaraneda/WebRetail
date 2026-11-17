using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Domain;
using RetailPOS.Infrastructure;
namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador")]
public sealed class ProveedoresController(TiendaDb db) : Controller
{
    public async Task<IActionResult> Index(string? q, int pagina = 1)
    {
        pagina = Math.Max(1, pagina); q = q?.Trim();
        var consulta = db.Proveedores.AsNoTracking().Where(x => q == null || x.Nombre.Contains(q) || (x.Rut != null && x.Rut.Contains(q)));
        ViewBag.Pagina = pagina; ViewBag.Total = await consulta.CountAsync(); ViewBag.Q = q;
        return View(await consulta.OrderBy(x => x.Nombre).Skip((pagina-1)*50).Take(50).ToListAsync());
    }
    public async Task<IActionResult> Editar(int id = 0) => View(id == 0 ? new Proveedor() : await db.Proveedores.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id) ?? throw new ReglaNegocioException("Proveedor inexistente."));
    [HttpPost] public async Task<IActionResult> Editar([Bind("Id,Nombre,Rut,Contacto,Direccion,Activo")] Proveedor form)
    {
        if (!ModelState.IsValid) return View(form);
        form.Rut = Validaciones.Rut(form.Rut);
        var actual = form.Id == 0 ? new Proveedor() : await db.Proveedores.FindAsync(form.Id) ?? throw new ReglaNegocioException("Proveedor inexistente.");
        actual.Nombre = form.Nombre.Trim(); actual.Rut = form.Rut; actual.Contacto = form.Contacto; actual.Direccion = form.Direccion; actual.Activo = form.Activo;
        if (form.Id == 0) db.Add(actual);
        await db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
}

[Authorize(Roles = "Administrador")]
public sealed class ComprasController(TiendaDb db, Operaciones operaciones) : Controller
{
    public async Task<IActionResult> Index(int pagina = 1)
    {
        pagina = Math.Max(1,pagina); ViewBag.Pagina = pagina; ViewBag.Total = await db.Compras.CountAsync();
        return View(await db.Compras.AsNoTracking().Include(x => x.Proveedor).Include(x => x.Bodega).OrderByDescending(x => x.Id).Skip((pagina-1)*50).Take(50).ToListAsync());
    }
    public async Task<IActionResult> Nueva()
    {
        ViewBag.Proveedores = await db.Proveedores.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.Bodegas = await db.Bodegas.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.Variantes = await db.Variantes.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Sku).ToListAsync();
        return View(new SolicitudCompra());
    }
    [HttpPost] public async Task<IActionResult> Nueva(SolicitudCompra form)
    {
        if (!ModelState.IsValid) throw new ReglaNegocioException("Revisa los datos de compra.");
        var id = await operaciones.ComprarAsync(form, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return RedirectToAction(nameof(Detalle), new { id });
    }
    public async Task<IActionResult> Detalle(int id)
    {
        var compra = await db.Compras.AsNoTracking().Include(x => x.Proveedor).Include(x => x.Bodega).Include(x => x.Detalles).ThenInclude(x => x.Variante).SingleOrDefaultAsync(x => x.Id == id);
        return compra is null ? NotFound() : View(compra);
    }
}

