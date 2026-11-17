using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Infrastructure;

namespace RetailPOS.Web.Controllers;

[Authorize(Roles = "Administrador,Vendedor")]
public sealed class CajaController(TiendaDb db, IOperaciones operaciones) : Controller
{
    public async Task<IActionResult> Index(DateOnly? fecha=null,string? cajero=null)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if(!ModelState.IsValid)throw new ReglaNegocioException("Selecciona una fecha válida.");
        var fechaConsulta=fecha??Precios.HoyChile;
        var usuarioConsulta=User.IsInRole("Administrador")?(string.IsNullOrWhiteSpace(cajero)?uid:cajero=="todos"?null:cajero):uid;
        if(usuarioConsulta is not null&&!await db.Users.AnyAsync(x=>x.Id==usuarioConsulta))return NotFound();
        ViewBag.Fecha=fechaConsulta;ViewBag.Cajero=usuarioConsulta??"todos";
        ViewBag.ResumenDiario=await operaciones.ResumenDiaAsync(fechaConsulta,usuarioConsulta);
        var caja = await db.SesionesCaja.AsNoTracking().SingleOrDefaultAsync(x => x.UsuarioId == uid && x.Cierre == null);
        ViewBag.Caja = caja;
        ViewBag.ResumenSesion=caja is null?null:await operaciones.ResumenCajaAsync(caja);
        ViewBag.Cuadratura = caja is null ? [] : await operaciones.CuadraturaAsync(caja);
        ViewBag.Efectivo = caja is null ? 0 : await db.Ventas.Where(x => x.SesionCajaId == caja.Id && x.MedioPago == "Efectivo").SumAsync(x => (decimal?)x.Total) ?? 0;
        ViewBag.Reembolsos = caja is null ? 0 : await db.Devoluciones.Where(x => x.SesionCajaId == caja.Id && x.MedioReembolso == "Efectivo").SumAsync(x => (decimal?)x.Reembolso) ?? 0;
        var sesiones = db.SesionesCaja.AsNoTracking().Include(x => x.Cuadraturas).AsQueryable();
        if (!User.IsInRole("Administrador")) sesiones = sesiones.Where(x => x.UsuarioId == uid);
        ViewBag.Usuarios = await db.Users.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Email ?? x.Id);
        return View(await sesiones.OrderByDescending(x => x.Id).Take(100).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Abrir(decimal fondo)
    {
        if (!ModelState.IsValid) throw new ReglaNegocioException("Indica un importe válido.");
        await operaciones.AbrirCajaAsync(fondo, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Cerrar(SolicitudCuadratura solicitud)
    {
        if (!ModelState.IsValid) throw new ReglaNegocioException("Indica un importe válido.");
        await operaciones.CerrarCajaDetalladaAsync(solicitud, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        TempData["Ok"] = "Caja cerrada y cuadratura guardada.";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Movimiento(Guid solicitudId, string tipo, decimal importe, string motivo)
    {
        if (!ModelState.IsValid) throw new ReglaNegocioException("Revisa los datos del movimiento de caja.");
        await operaciones.MovimientoCajaAsync(solicitudId, tipo, importe, motivo, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        TempData["Ok"] = "Movimiento de efectivo registrado.";
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Detalle(int id)
    {
        var caja = await db.SesionesCaja.AsNoTracking().Include(x => x.Cuadraturas).SingleOrDefaultAsync(x => x.Id == id);
        if (caja is null) return NotFound();
        if (!User.IsInRole("Administrador") && caja.UsuarioId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();
        ViewBag.Filas = caja.Cierre is null ? await operaciones.CuadraturaAsync(caja) : caja.Cuadraturas;
        ViewBag.Resumen=await operaciones.ResumenCajaAsync(caja);
        ViewBag.Movimientos = await db.MovimientosCaja.AsNoTracking().Where(x => x.SesionCajaId == id).OrderBy(x => x.Id).ToListAsync();
        ViewBag.Vendedor = await db.Users.Where(x => x.Id == caja.UsuarioId).Select(x => x.Email).SingleOrDefaultAsync() ?? caja.UsuarioId;
        ViewBag.Revisor = caja.RevisadoPor is null ? null : await db.Users.Where(x => x.Id == caja.RevisadoPor).Select(x => x.Email).SingleOrDefaultAsync() ?? caja.RevisadoPor;
        return View(caja);
    }
    [HttpPost, Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Revisar(int id)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var ahora = DateTimeOffset.UtcNow;
        var actualizadas = await db.SesionesCaja.Where(x => x.Id == id && x.Cierre != null && x.Revision == null).ExecuteUpdateAsync(s => s.SetProperty(x => x.RevisadoPor, uid).SetProperty(x => x.Revision, ahora));
        if (actualizadas == 0) throw new ReglaNegocioException("La caja no existe, sigue abierta o ya fue revisada.");
        TempData["Ok"] = "Cuadratura marcada como revisada.";
        return RedirectToAction(nameof(Detalle), new { id });
    }
}

