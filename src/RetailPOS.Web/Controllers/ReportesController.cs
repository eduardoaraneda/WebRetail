using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Infrastructure;
using ClosedXML.Excel;

namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador")]
public sealed class ReportesController(TiendaDb db) : Controller
{
    private IQueryable<FilaReporte> Consulta(FiltroReporte f)
    {
        if (!ModelState.IsValid || f.Desde > f.Hasta) throw new RetailPOS.Application.ReglaNegocioException("Revisa el rango de fechas.");
        var desde = Limite(f.Desde); var hasta = Limite(f.Hasta.AddDays(1));
        return db.DetallesVenta.AsNoTracking().Where(x => x.Venta.Fecha >= desde && x.Venta.Fecha < hasta && (f.Vendedor == null || x.Venta.UsuarioId == f.Vendedor) && (f.Marca == null || x.MarcaVenta == f.Marca) && (f.Tienda == null || x.Venta.Bodega.Tienda == f.Tienda))
            .Select(x => new FilaReporte { VentaId = x.VentaId, Fecha = x.Venta.Fecha, Vendedor = x.Venta.UsuarioId, Tienda = x.Venta.Bodega.Tienda, Sku = x.Variante.Sku, Descripcion = x.Descripcion, Marca = x.MarcaVenta, Cantidad = x.Cantidad, Devueltas = db.Devoluciones.Where(d => d.DetalleVentaId == x.Id).Sum(d => (int?)d.Cantidad) ?? 0, Precio = x.PrecioUnitario, Costo = x.CostoUnitario });
    }
    private static DateTimeOffset Limite(DateOnly fecha)
    {
        var local = fecha.ToDateTime(TimeOnly.MinValue,DateTimeKind.Unspecified); var zona = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");
        while (zona.IsInvalidTime(local)) local = local.AddMinutes(30);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local,zona));
    }
    public async Task<IActionResult> Index(FiltroReporte filtro)
    {
        var q = Consulta(filtro); var filas = await q.OrderByDescending(x => x.VentaId).ToListAsync();
        ViewBag.Filtro = filtro; ViewBag.Total = filas.Count; ViewBag.Ingresos = filas.Sum(x => x.Neto);
        ViewBag.SinCosto = filas.Count(x => x.Costo is null); ViewBag.Utilidad = filas.Where(x => x.Costo.HasValue).Sum(x => x.Utilidad ?? 0);
        ViewBag.Marcas = await db.Marcas.OrderBy(x => x.Nombre).Select(x => x.Nombre).ToListAsync();
        ViewBag.Tiendas = await db.Bodegas.Select(x => x.Tienda).Distinct().OrderBy(x => x).ToListAsync();
        ViewBag.Usuarios = await db.Users.ToDictionaryAsync(x => x.Id,x => x.Email ?? x.Id);
        filtro.Pagina = Math.Max(1,filtro.Pagina);
        return View(filas.Skip((filtro.Pagina-1)*50).Take(50).ToList());
    }
    public async Task<IActionResult> Excel(FiltroReporte filtro)
    {
        var filas = await Consulta(filtro).OrderByDescending(x => x.VentaId).Take(100001).ToListAsync();
        if (filas.Count > 100000) throw new RetailPOS.Application.ReglaNegocioException("Acota los filtros para exportar hasta 100.000 líneas.");
        using var libro = new XLWorkbook(); var hoja = libro.Worksheets.Add("Ventas y utilidad");
        var encabezados = new[] { "Venta", "Fecha UTC", "Vendedor ID", "Tienda", "SKU", "Marca", "Descripción", "Vendidas", "Devueltas", "Precio CLP", "Ingreso neto CLP", "Costo unitario CLP", "Utilidad estimada CLP" };
        for (var c=0;c<encabezados.Length;c++) hoja.Cell(1,c+1).Value = encabezados[c];
        for (var i=0;i<filas.Count;i++)
        {
            var f=filas[i]; var r=i+2;
            hoja.Cell(r,1).Value=f.VentaId; hoja.Cell(r,2).Value=f.Fecha.UtcDateTime; hoja.Cell(r,3).Value=f.Vendedor; hoja.Cell(r,4).Value=f.Tienda; hoja.Cell(r,5).Value=f.Sku; hoja.Cell(r,6).Value=f.Marca; hoja.Cell(r,7).Value=f.Descripcion; hoja.Cell(r,8).Value=f.Cantidad; hoja.Cell(r,9).Value=f.Devueltas; hoja.Cell(r,10).Value=f.Precio; hoja.Cell(r,11).Value=f.Neto;
            if(f.Costo.HasValue) { hoja.Cell(r,12).Value=f.Costo.Value; hoja.Cell(r,13).Value=f.Utilidad!.Value; }
        }
        hoja.Row(1).Style.Font.Bold=true; hoja.SheetView.FreezeRows(1); hoja.Columns().AdjustToContents(10,60); hoja.Column(2).Style.DateFormat.Format="dd/mm/yyyy hh:mm";
        using var memoria=new MemoryStream(); libro.SaveAs(memoria); return File(memoria.ToArray(),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","ventas-utilidad.xlsx");
    }
}
public sealed record VentaResumen(string Descripcion, int Cantidad, decimal Total);
public sealed class FiltroReporte
{
    public DateOnly Desde { get; set; } = new(Precios.HoyChile.Year,Precios.HoyChile.Month,1);
    public DateOnly Hasta { get; set; } = Precios.HoyChile;
    public string? Vendedor { get; set; }
    public string? Marca { get; set; }
    public string? Tienda { get; set; }
    public int Pagina { get; set; } = 1;
}
public sealed class FilaReporte
{
    public int VentaId { get; set; } public DateTimeOffset Fecha { get; set; }
    public string Vendedor { get; set; }=""; public string Tienda { get; set; }=""; public string Sku { get; set; }=""; public string Marca { get; set; }=""; public string Descripcion { get; set; }="";
    public int Cantidad { get; set; } public int Devueltas { get; set; } public decimal Precio { get; set; } public decimal? Costo { get; set; }
    public decimal Neto => (Cantidad-Devueltas)*Precio;
    public decimal? Utilidad => Costo is null ? null : (Cantidad-Devueltas)*(Precio-Costo.Value);
}

