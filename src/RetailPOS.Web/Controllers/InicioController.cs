using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Infrastructure;

namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador,Vendedor,Bodega")]
public sealed class InicioController(TiendaDb db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Productos = await db.Productos.CountAsync();
        ViewBag.Variantes = await db.Variantes.CountAsync();
        ViewBag.Unidades = await db.Existencias.SumAsync(x => (int?)x.Cantidad) ?? 0;
        ViewBag.Ventas = await db.Ventas.CountAsync();
        ViewBag.Alertas = await db.Existencias.CountAsync(x=>x.Cantidad<=x.StockMinimo);
        ViewBag.Revisiones = await db.RevisionesStock.CountAsync(x=>x.Estado=="Revision");
        return View();
    }
}

