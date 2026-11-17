using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Infrastructure;

namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador,Vendedor")]
public sealed class VentasController(TiendaDb db, IOperaciones operaciones) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Ventas.AsNoTracking().Include(x => x.Cliente).OrderByDescending(x => x.Id).Take(200).ToListAsync());
    [HttpGet] public async Task<IActionResult> Nueva()
    {
        ViewBag.Bodegas = await db.Bodegas.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.Clientes = await db.Clientes.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.Stock = await db.Existencias.AsNoTracking().Include(x => x.Variante).ThenInclude(x => x.Producto).Include(x => x.Bodega)
            .Where(x => x.Cantidad > 0 && x.Variante.Activo && x.Variante.Producto.Activo
                && db.Marcas.Any(m => m.Nombre == x.Variante.Producto.Marca && m.Activa)
                && db.Tallas.Any(t => t.Valor == x.Variante.Talla && t.Sistema == x.Variante.SistemaTalla && t.Activa))
            .OrderBy(x => x.Variante.Sku).Take(1000).ToListAsync();
        var ofertas = await new Precios(db).VigentesAsync();
        var stock = (List<RetailPOS.Domain.Existencia>)ViewBag.Stock;
        ViewBag.Precios = stock.Select(x => x.Variante).DistinctBy(x => x.Id).ToDictionary(x => x.Id, x => RetailPOS.Domain.MotorOfertas.Calcular(x, ofertas, Precios.HoyChile));
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        ViewBag.CajaAbierta = await db.SesionesCaja.AnyAsync(x => x.UsuarioId == uid && x.Cierre == null);
        return View(new SolicitudVenta());
    }
    [HttpPost] public async Task<IActionResult> Nueva(SolicitudVenta solicitud)
    {
        if (!ModelState.IsValid) throw new ReglaNegocioException("Completa la bodega, las líneas y las cantidades.");
        var id = await operaciones.VenderAsync(solicitud, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return RedirectToAction(nameof(Detalle), new { id });
    }
    public async Task<IActionResult> Detalle(int id)
    {
        var venta = await db.Ventas.AsNoTracking().Include(x => x.Cliente).Include(x => x.Bodega).Include(x => x.Detalles).Include(x=>x.Pagos).SingleOrDefaultAsync(x => x.Id == id);
        if (venta is null) return NotFound();
        var modelos=await db.Variantes.Where(x=>venta.Detalles.Select(d=>d.VarianteId).Contains(x.Id)).Select(x=>x.ProductoId).ToListAsync();
        ViewBag.VariantesCambio=await db.Variantes.AsNoTracking().Where(x=>x.Activo && modelos.Contains(x.ProductoId)).OrderBy(x=>x.Sku).ToListAsync();
        ViewBag.Documentos = await db.Documentos.AsNoTracking().Where(x => x.VentaId == id).OrderBy(x => x.Id).ToListAsync();
        ViewBag.Devueltas = await db.Devoluciones.AsNoTracking().Where(x => x.DetalleVenta.VentaId == id).GroupBy(x => x.DetalleVentaId).Select(x => new { Id = x.Key, Cantidad = x.Sum(d => d.Cantidad) }).ToDictionaryAsync(x => x.Id, x => x.Cantidad);
        return View(venta);
    }
}

