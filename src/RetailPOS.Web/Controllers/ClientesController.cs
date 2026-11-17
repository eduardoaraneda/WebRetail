using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Domain;
using RetailPOS.Infrastructure;

namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador,Vendedor")]
public sealed class ClientesController(TiendaDb db) : Controller
{
    public async Task<IActionResult> Index(string? q, int pagina=1)
    {
        pagina=Math.Max(1,pagina); var consulta=db.Clientes.AsNoTracking().Where(x => q==null || x.Nombre.Contains(q) || (x.Rut!=null && x.Rut.Contains(q)));
        ViewBag.Q=q; ViewBag.Pagina=pagina; ViewBag.Total=await consulta.CountAsync(); return View(await consulta.OrderBy(x=>x.Nombre).Skip((pagina-1)*50).Take(50).ToListAsync());
    }
    public async Task<IActionResult> Editar(int id) => View(await db.Clientes.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id) ?? throw new ReglaNegocioException("Cliente inexistente."));
    [HttpPost] public async Task<IActionResult> Editar([Bind("Id,Nombre,Rut,Giro,Direccion,Comuna")] Cliente form)
    {
        if(!ModelState.IsValid) return View(form);
        var actual=await db.Clientes.FindAsync(form.Id) ?? throw new ReglaNegocioException("Cliente inexistente.");
        actual.Nombre=form.Nombre.Trim(); actual.Rut=Validaciones.Rut(form.Rut); actual.Giro=form.Giro; actual.Direccion=form.Direccion; actual.Comuna=form.Comuna;
        await db.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Crear([Bind("Nombre,Rut,Giro,Direccion,Comuna")] Cliente cliente)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(cliente.Nombre)) throw new ReglaNegocioException("Revisa los datos del cliente.");
        cliente.Nombre = cliente.Nombre.Trim();
        if (!string.IsNullOrWhiteSpace(cliente.Rut))
        {
            var rut = cliente.Rut.Replace(".", "").Replace("-", "").Trim().ToUpperInvariant();
            if (rut.Length is < 2 or > 9 || !int.TryParse(rut[..^1], out var numero) || numero <= 0) throw new ReglaNegocioException("RUT inválido.");
            var suma = 0; var factor = 2;
            for (var n = numero; n > 0; n /= 10) { suma += n % 10 * factor; factor = factor == 7 ? 2 : factor + 1; }
            var digito = 11 - suma % 11;
            var esperado = digito == 11 ? "0" : digito == 10 ? "K" : digito.ToString();
            if (rut[^1].ToString() != esperado) throw new ReglaNegocioException("El dígito verificador del RUT no coincide.");
            cliente.Rut = $"{numero}-{esperado}";
        }
        db.Add(cliente); await db.SaveChangesAsync(); TempData["Ok"] = "Cliente registrado.";
        return RedirectToAction(nameof(Index));
    }
}

