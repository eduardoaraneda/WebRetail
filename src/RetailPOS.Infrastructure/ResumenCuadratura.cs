using System.Data;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Domain;
namespace RetailPOS.Infrastructure;

public sealed partial class Operaciones
{
    public static readonly string[] MediosCaja=["Efectivo","Tarjeta","Transferencia","Otros"];
    private static void ValidarDiaCaja(SesionCaja caja)
    {
        if(caja.FechaOperativa!=Precios.HoyChile)throw new ReglaNegocioException("La caja abierta corresponde a otro día. Ciérrala y abre una nueva antes de registrar operaciones de hoy.");
    }
    public static DateTimeOffset InicioDiaChile(DateOnly fecha)
    {
        var zona=TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");var local=fecha.ToDateTime(TimeOnly.MinValue,DateTimeKind.Unspecified);
        while(zona.IsInvalidTime(local))local=local.AddMinutes(30);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local,zona));
    }
    public async Task<ResumenCuadratura> ResumenCajaAsync(SesionCaja caja,CancellationToken ct=default)
    {
        var resultado=await CrearResumenAsync(db.Ventas.Where(x=>x.SesionCajaId==caja.Id),db.Devoluciones.Where(x=>x.SesionCajaId==caja.Id),db.Pagos.Where(x=>x.SesionCajaId==caja.Id),db.Reembolsos.Where(x=>x.SesionCajaId==caja.Id),ct);
        resultado.Fecha=caja.FechaOperativa;resultado.Alcance="Caja #"+caja.Id;
        var filas=caja.Cierre is null?await CuadraturaAsync(caja,ct):await db.Cuadraturas.AsNoTracking().Where(x=>x.SesionCajaId==caja.Id).ToListAsync(ct);
        resultado.Fondo=caja.FondoInicial;resultado.IngresosAdicionales=filas.Sum(x=>x.Ingresos);resultado.Retiros=filas.Sum(x=>x.Retiros);
        resultado.DeclaradoBruto=caja.Cierre is not null&&filas.Count>0?filas.Sum(x=>x.Contado):null;
        resultado.DiferenciasPorMedio=filas.Any(x=>x.Contado!=x.Esperado);resultado.CajasAbiertas=caja.Cierre is null?1:0;
        return resultado;
    }
    public async Task<ResumenCuadratura> ResumenDiaAsync(DateOnly fecha,string? usuarioId,CancellationToken ct=default)
    {
        if(fecha.Year<2000||fecha.Year>9998)throw new ReglaNegocioException("Selecciona una fecha válida.");
        var desde=InicioDiaChile(fecha);var hasta=InicioDiaChile(fecha.AddDays(1));
        await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct):null;
        var ventas=db.Ventas.Where(x=>x.Fecha>=desde&&x.Fecha<hasta&&(usuarioId==null||x.UsuarioId==usuarioId));
        var devoluciones=db.Devoluciones.Where(x=>x.Fecha>=desde&&x.Fecha<hasta&&(usuarioId==null||x.SesionCaja.UsuarioId==usuarioId));
        var pagos=db.Pagos.Where(x=>x.Fecha>=desde&&x.Fecha<hasta&&(usuarioId==null||x.SesionCaja.UsuarioId==usuarioId));
        var reembolsos=db.Reembolsos.Where(x=>x.Fecha>=desde&&x.Fecha<hasta&&(usuarioId==null||x.SesionCaja.UsuarioId==usuarioId));
        var resultado=await CrearResumenAsync(ventas,devoluciones,pagos,reembolsos,ct);
        var sesiones=db.SesionesCaja.Where(x=>x.FechaOperativa==fecha&&(usuarioId==null||x.UsuarioId==usuarioId));
        var filas=await db.Cuadraturas.AsNoTracking().Where(x=>sesiones.Any(c=>c.Id==x.SesionCajaId)).ToListAsync(ct);
        resultado.Fecha=fecha;resultado.Alcance=usuarioId is null?"Todos los cajeros":"Cajero seleccionado";
        resultado.Fondo=await sesiones.SumAsync(x=>(decimal?)x.FondoInicial,ct)??0;
        var movimientos=db.MovimientosCaja.Where(x=>sesiones.Any(c=>c.Id==x.SesionCajaId));
        resultado.IngresosAdicionales=await movimientos.Where(x=>x.Tipo=="Ingreso").SumAsync(x=>(decimal?)x.Importe,ct)??0;
        resultado.Retiros=await movimientos.Where(x=>x.Tipo=="Retiro").SumAsync(x=>(decimal?)x.Importe,ct)??0;
        resultado.CajasAbiertas=await sesiones.CountAsync(x=>x.Cierre==null,ct);
        var sinDeclaracion=await sesiones.CountAsync(x=>x.Cierre!=null&&!db.Cuadraturas.Any(f=>f.SesionCajaId==x.Id),ct);
        resultado.DeclaradoBruto=await sesiones.AnyAsync(ct)&&resultado.CajasAbiertas==0&&sinDeclaracion==0?filas.Sum(x=>x.Contado):null;
        resultado.DiferenciasPorMedio=filas.Any(x=>x.Contado!=x.Esperado);
        if(tx is not null)await tx.CommitAsync(ct);return resultado;
    }
    private async Task<ResumenCuadratura> CrearResumenAsync(IQueryable<Venta> ventas,IQueryable<Devolucion> devoluciones,IQueryable<PagoVenta> pagos,IQueryable<ReembolsoPago> reembolsos,CancellationToken ct)
    {
        var resumen=new ResumenCuadratura{VentasCantidad=await ventas.CountAsync(ct)};
        var documentos=await db.Documentos.AsNoTracking().Where(x=>(x.Tipo==33||x.Tipo==39)&&ventas.Any(v=>v.Id==x.VentaId)).GroupBy(x=>x.Tipo).Select(g=>new{Tipo=g.Key,Cantidad=g.Count(),Total=g.Sum(x=>x.Total)}).ToListAsync(ct);
        foreach(var (tipo,nombre) in new[]{(39,"Boletas"),(33,"Facturas")})
        {
            var d=documentos.SingleOrDefault(x=>x.Tipo==tipo);resumen.Documentos.Add(new(){Tipo=tipo,Nombre=nombre,Cantidad=d?.Cantidad??0,Total=d?.Total??0});
        }
        resumen.VentasBrutas=resumen.Documentos.Sum(x=>x.Total);
        var totalVentas=await ventas.SumAsync(x=>(decimal?)x.Total,ct)??0;
        if(totalVentas!=resumen.VentasBrutas)throw new ReglaNegocioException("Hay ventas sin documento de venta o con totales inconsistentes. Revisa sus boletas y facturas antes de cuadrar.");
        resumen.NotasCreditoCantidad=await devoluciones.CountAsync(ct);resumen.NotasCredito=await devoluciones.SumAsync(x=>(decimal?)x.Reembolso,ct)??0;
        resumen.CobrosDebito=await pagos.Where(x=>x.DocumentoId!=null).SumAsync(x=>(decimal?)x.Importe,ct)??0;
        resumen.SaldoCambios=await pagos.Where(x=>x.Medio=="SaldoCambio").SumAsync(x=>(decimal?)x.Importe,ct)??0;
        var agrupados=await pagos.Where(x=>x.Medio!="SaldoCambio").GroupBy(x=>new{x.Medio,x.DocumentoId}).Select(g=>new{g.Key.Medio,g.Key.DocumentoId,Cantidad=g.Count(),Total=g.Sum(x=>x.Importe)}).ToListAsync(ct);
        var devueltos=await reembolsos.GroupBy(x=>x.Medio).Select(g=>new{Medio=g.Key,Total=g.Sum(x=>x.Importe)}).ToListAsync(ct);
        foreach(var medio in MediosCaja)resumen.Ingresos.Add(new(){Medio=medio,Operaciones=agrupados.Where(x=>x.Medio==medio).Sum(x=>x.Cantidad),Ventas=agrupados.Where(x=>x.Medio==medio&&x.DocumentoId==null).Sum(x=>x.Total),CobrosDebito=agrupados.Where(x=>x.Medio==medio&&x.DocumentoId!=null).Sum(x=>x.Total),Reembolsos=devueltos.SingleOrDefault(x=>x.Medio==medio)?.Total??0});
        return resumen;
    }
}
