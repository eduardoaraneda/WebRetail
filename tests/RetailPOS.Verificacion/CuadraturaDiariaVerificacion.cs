using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Domain;
using RetailPOS.Infrastructure;
internal static class CuadraturaDiariaVerificacion
{
    public static async Task EjecutarAsync(DbContextOptions<TiendaDb> opciones)
    {
        void Check(bool ok,string mensaje){if(!ok)throw new Exception(mensaje);Console.WriteLine("OK CUADRATURA: "+mensaje);}
        int variante,bodega,cliente,ventaBoleta;
        decimal totalDiaPrevio;
        await using(var db=new TiendaDb(opciones))totalDiaPrevio=(await new Operaciones(db).ResumenDiaAsync(Precios.HoyChile,null)).TotalSistema;
        await using(var db=new TiendaDb(opciones))
        {
            var p=new Producto{Marca="Prueba",Modelo="Cuadratura diaria"};var v=new Variante{Producto=p,Sku="CUAD-DIA",Color="Azul",Talla="40",SistemaTalla="EU",Precio=50000};var b=new Bodega{Nombre="Cuadratura diaria",Tienda="Tienda prueba"};var c=new Cliente{Nombre="Cliente factura prueba",Rut="12345678-5",Giro="Comercio",Direccion="Dirección de prueba",Comuna="Santiago"};
            db.AddRange(v,b,c);await db.SaveChangesAsync();variante=v.Id;bodega=b.Id;cliente=c.Id;
            db.Add(new Existencia{VarianteId=variante,BodegaId=bodega,Cantidad=50,CostoPromedio=20000});await db.SaveChangesAsync();
            await new Operaciones(db).AbrirCajaAsync(10000,"caja-dia-a");await new Operaciones(db).AbrirCajaAsync(0,"caja-dia-b");
        }
        await using(var db=new TiendaDb(opciones))ventaBoleta=await new Operaciones(db).VenderAsync(new(){BodegaId=bodega,Lineas=[new(){VarianteId=variante,Cantidad=2}],Pagos=[new(){Medio="Efectivo",Importe=60000},new(){Medio="Tarjeta",Importe=40000}]},"caja-dia-a");
        await using(var db=new TiendaDb(opciones))await new Operaciones(db).VenderAsync(new(){BodegaId=bodega,ClienteId=cliente,TipoDocumento=33,MedioPago="Transferencia",Lineas=[new(){VarianteId=variante,Cantidad=1}]},"caja-dia-a");
        await using(var db=new TiendaDb(opciones))await new Operaciones(db).VenderAsync(new(){BodegaId=bodega,MedioPago="Otros",Lineas=[new(){VarianteId=variante,Cantidad=1}]},"caja-dia-b");
        await using(var db=new TiendaDb(opciones))
        {
            var ops=new Operaciones(db);var doc=await db.Documentos.SingleAsync(x=>x.VentaId==ventaBoleta&&x.Tipo==39);await ops.CrearDocumentoAsync(ventaBoleta,52,doc.Id,"Despacho de prueba","caja-dia-a");
            var linea=await db.DetallesVenta.SingleAsync(x=>x.VentaId==ventaBoleta);await ops.DevolverAsync(Guid.NewGuid(),linea.Id,1,true,"Devolución parcial","caja-dia-a");
            var debito=await ops.DebitarAsync(Guid.NewGuid(),doc.Id,10000,"Ajuste de prueba","caja-dia-a");await ops.CobrarDebitoAsync(Guid.NewGuid(),debito,4000,"Otros","caja-dia-a");
            await ops.MovimientoCajaAsync(Guid.NewGuid(),"Ingreso",2000,"Fondo adicional","caja-dia-a");await ops.MovimientoCajaAsync(Guid.NewGuid(),"Retiro",1000,"Retiro parcial","caja-dia-a");
        }
        var hoy=Precios.HoyChile;
        await using(var db=new TiendaDb(opciones))
        {
            var ops=new Operaciones(db);var caja=await db.SesionesCaja.SingleAsync(x=>x.UsuarioId=="caja-dia-a"&&x.Cierre==null);var r=await ops.ResumenCajaAsync(caja);
            Check(r.Documentos.Single(x=>x.Tipo==39).Cantidad==1&&r.Documentos.Single(x=>x.Tipo==39).Total==100000&&r.Documentos.Single(x=>x.Tipo==33).Cantidad==1&&r.Documentos.Single(x=>x.Tipo==33).Total==50000,"Boletas y facturas: cantidad y monto, sin duplicar pagos mixtos ni guías");
            Check(r.VentasBrutas==150000&&r.NotasCredito==50000&&r.TotalVentasNeto==100000&&r.CobrosDebito==4000&&r.TotalSistema==104000&&r.TotalPagos==104000,"Venta neta y recaudación separan devoluciones y solo débitos cobrados");
            Check(r.Ingresos.Single(x=>x.Medio=="Efectivo").Ventas==60000&&r.Ingresos.Single(x=>x.Medio=="Tarjeta").Ventas==40000&&r.Ingresos.Single(x=>x.Medio=="Transferencia").Ventas==50000&&r.Ingresos.Single(x=>x.Medio=="Otros").CobrosDebito==4000,"Desglose efectivo, tarjetas, transferencias y otros medios");
            await ops.CerrarCajaDetalladaAsync(new(){Contado=21000,Tarjeta=40000,Transferencia=50000,Otros=4000},"caja-dia-a");
        }
        await using(var db=new TiendaDb(opciones))
        {
            var ops=new Operaciones(db);var r=await ops.ResumenDiaAsync(hoy,"caja-dia-a");
            Check(r.DeclaradoBruto==115000&&r.DeclaradoNeto==104000&&r.Diferencia==0&&r.Estado=="Cuadrada","Total ingresado por cajero cuadra con ventas ajustando fondo, ingresos y retiros");
            await ops.AbrirCajaAsync(0,"caja-dia-a");await ops.VenderAsync(new(){BodegaId=bodega,Lineas=[new(){VarianteId=variante,Cantidad=1}]},"caja-dia-a");
        }
        await using(var db=new TiendaDb(opciones))
        {
            var ops=new Operaciones(db);var r=await ops.ResumenDiaAsync(hoy,"caja-dia-a");
            Check(r.Documentos.Single(x=>x.Tipo==39).Cantidad==2&&r.DeclaradoNeto is null&&r.CajasAbiertas==1,"El resumen diario agrupa sesiones sin declarar como cerrado un día con caja abierta");
            await ops.CerrarCajaDetalladaAsync(new(){Contado=49999,Tarjeta=0,Transferencia=0,Otros=0,Observacion="Faltante de un peso"},"caja-dia-a");
        }
        await using(var db=new TiendaDb(opciones))
        {
            var r=await new Operaciones(db).ResumenDiaAsync(hoy,"caja-dia-a");var otro=await new Operaciones(db).ResumenDiaAsync(hoy,"caja-dia-b");
            Check(r.TotalSistema==154000&&r.DeclaradoNeto==153999&&r.Diferencia==-1&&r.Estado=="Faltante","Faltante diario agrega las declaraciones de todas las sesiones del cajero");
            Check(otro.VentasBrutas==50000&&otro.Documentos.Single(x=>x.Tipo==39).Cantidad==1,"Los totales se separan por cajero");
            await new Operaciones(db).CerrarCajaDetalladaAsync(new(){Contado=0,Tarjeta=0,Transferencia=0,Otros=50000},"caja-dia-b");
            var global=await new Operaciones(db).ResumenDiaAsync(hoy,null);
            Check(global.TotalSistema-totalDiaPrevio==204000,"Vista de todos los cajeros agrega exactamente las ventas netas del día");
            await new Operaciones(db).AbrirCajaAsync(0,"caja-dia-anterior");
        }
        int vieja;
        await using(var db=new TiendaDb(opciones))vieja=await new Operaciones(db).VenderAsync(new(){BodegaId=bodega,MedioPago="Otros",Lineas=[new(){VarianteId=variante,Cantidad=1}]},"caja-dia-anterior");
        await using(var db=new TiendaDb(opciones))
        {
            var instante=Operaciones.InicioDiaChile(hoy).AddTicks(-10);var caja=await db.SesionesCaja.SingleAsync(x=>x.UsuarioId=="caja-dia-anterior"&&x.Cierre==null);caja.FechaOperativa=hoy.AddDays(-1);caja.Apertura=instante;
            await db.Ventas.Where(x=>x.Id==vieja).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Fecha,instante));await db.Pagos.Where(x=>x.VentaId==vieja).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Fecha,instante));await db.Documentos.Where(x=>x.VentaId==vieja).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Fecha,instante));await db.SaveChangesAsync();
            await new Operaciones(db).CerrarCajaDetalladaAsync(new(){Contado=0,Tarjeta=0,Transferencia=0,Otros=50000},"caja-dia-anterior");
        }
        await using(var db=new TiendaDb(opciones))
        {
            var ops=new Operaciones(db);var actual=await ops.ResumenDiaAsync(hoy,"caja-dia-anterior");var anterior=await ops.ResumenDiaAsync(hoy.AddDays(-1),"caja-dia-anterior");
            Check(actual.VentasCantidad==0&&anterior.VentasBrutas==50000&&anterior.Diferencia==0,"El día se filtra por hora de Chile, sin incluir ventas anteriores");
            await ops.AbrirCajaAsync(0,"caja-dia-vencida");
            var caja=await db.SesionesCaja.SingleAsync(x=>x.UsuarioId=="caja-dia-vencida"&&x.Cierre==null);caja.FechaOperativa=hoy.AddDays(-1);await db.SaveChangesAsync();
        }
        await using(var db=new TiendaDb(opciones))
        {
            try{await new Operaciones(db).VenderAsync(new(){BodegaId=bodega,Lineas=[new(){VarianteId=variante,Cantidad=1}]},"caja-dia-vencida");throw new Exception("Se permitió vender en caja del día anterior");}catch(ReglaNegocioException){Console.WriteLine("OK CUADRATURA: Caja del día anterior exige cierre antes de vender hoy");}
        }
    }
}
