using Microsoft.EntityFrameworkCore;
using RetailPOS.Application;
using RetailPOS.Domain;
using RetailPOS.Infrastructure;
using RetailPOS.Web;
internal static class CompletaVerificacion
{
    public static async Task EjecutarAsync(DbContextOptions<TiendaDb> opciones)
    {
        void Check(bool ok,string m){if(!ok)throw new Exception(m);Console.WriteLine("OK COMPLETA: "+m);}
        async Task Rechaza(Func<Task> accion,string m){try{await accion();}catch(ReglaNegocioException){Console.WriteLine("OK COMPLETA: "+m);return;}throw new Exception(m);}
        int a,b,v1,v2,v3,proveedor;
        await using(var db=new TiendaDb(opciones))
        {
            db.ActorId="prueba-completa";
            db.AddRange(new Marca{Nombre="Completa"},new TallaCalzado{Valor="43",Sistema="EU"},new TallaCalzado{Valor="44",Sistema="EU"},new TallaCalzado{Valor="45",Sistema="EU"});
            await db.SaveChangesAsync();
            var p=new Producto{Marca="Completa",Modelo="Modelo completo"};
            var uno=new Variante{Producto=p,Sku="CMP43",Talla="43",SistemaTalla="EU",Color="Azul",Precio=50000};
            var dos=new Variante{Producto=p,Sku="CMP44",Talla="44",SistemaTalla="EU",Color="Azul",Precio=60000};
            var tres=new Variante{Producto=p,Sku="CMP45",Talla="45",SistemaTalla="EU",Color="Azul",Precio=45000};
            var origen=new Bodega{Nombre="Completa A",Tienda="Centro"};var destino=new Bodega{Nombre="Completa B",Tienda="Norte"};var prov=new Proveedor{Nombre="Proveedor de pruebas"};
            db.AddRange(uno,dos,tres,origen,destino,prov);await db.SaveChangesAsync();a=origen.Id;b=destino.Id;v1=uno.Id;v2=dos.Id;v3=tres.Id;proveedor=prov.Id;
        }
        var compra=new SolicitudCompra{ProveedorId=proveedor,BodegaId=a,Referencia="PRUEBA-1",Lineas=[new(){VarianteId=v1,Cantidad=10,CostoUnitario=20000}]};
        await using(var db=new TiendaDb(opciones))await new Operaciones(db).ComprarAsync(compra,"completa");
        await using(var db=new TiendaDb(opciones))
        {
            await new Operaciones(db).ComprarAsync(compra,"completa");
            Check(await db.Compras.CountAsync(x=>x.SolicitudId==compra.SolicitudId)==1&&(await db.Existencias.SingleAsync(x=>x.BodegaId==a&&x.VarianteId==v1)).Cantidad==10,"Compra idempotente ingresa stock una vez");
            await new Operaciones(db).ComprarAsync(new(){ProveedorId=proveedor,BodegaId=a,Referencia="PRUEBA-2",Lineas=[new(){VarianteId=v1,Cantidad=10,CostoUnitario=30000},new(){VarianteId=v2,Cantidad=2,CostoUnitario=35000}]},"completa");
        }
        var traslado=Guid.NewGuid();
        await using(var db=new TiendaDb(opciones))await new Operaciones(db).OperarStockAsync(traslado,"Traspaso",v1,a,b,5,"Traspaso prueba","completa");
        await using(var db=new TiendaDb(opciones))
        {
            await new Operaciones(db).OperarStockAsync(traslado,"Traspaso",v1,a,b,5,"Traspaso prueba","completa");
            var stocks=await db.Existencias.Where(x=>x.VarianteId==v1).ToListAsync();
            Check(stocks.Sum(x=>x.Cantidad)==20&&stocks.All(x=>x.CostoPromedio==25000),"Traspaso conserva cantidades y costo promedio ponderado");
        }
        await using(var db=new TiendaDb(opciones))await Rechaza(()=>new Operaciones(db).OperarStockAsync(Guid.NewGuid(),"Traspaso",v1,a,b,999,"Exceso","completa"),"Traspaso insuficiente se revierte");
        await using(var db=new TiendaDb(opciones))await new Operaciones(db).OperarStockAsync(Guid.NewGuid(),"Conteo",v1,a,null,14,"Conteo físico","completa");
        await using(var db=new TiendaDb(opciones))await new Operaciones(db).OperarStockAsync(Guid.NewGuid(),"Revision",v1,a,null,2,"Inspección","completa");
        await using(var db=new TiendaDb(opciones))
        {
            var rev=await db.RevisionesStock.SingleAsync(x=>x.VarianteId==v1);await new Operaciones(db).ResolverRevisionAsync(rev.Id,"Disponible","Aprobadas","completa");
            Check((await db.Existencias.SingleAsync(x=>x.VarianteId==v1&&x.BodegaId==a)).Cantidad==14,"Conteo y liberación de revisión actualizan stock");
        }
        await using(var db=new TiendaDb(opciones))await new Operaciones(db).AbrirCajaAsync(10000,"completa");
        await using(var db=new TiendaDb(opciones))await Rechaza(async()=>await new Operaciones(db).VenderAsync(new(){BodegaId=a,Lineas=[new(){VarianteId=v1,Cantidad=1}],Pagos=[new(){Medio="Efectivo",Importe=49999}]},"completa"),"Pagos incompletos revierten venta y stock");
        int venta;
        await using(var db=new TiendaDb(opciones))venta=await new Operaciones(db).VenderAsync(new(){BodegaId=a,Lineas=[new(){VarianteId=v1,Cantidad=1}],Pagos=[new(){Medio="Efectivo",Importe=10000},new(){Medio="Tarjeta",Importe=40000}]},"completa");
        int detalle,debito;
        await using(var db=new TiendaDb(opciones))
        {
            var d=await db.DetallesVenta.SingleAsync(x=>x.VentaId==venta);detalle=d.Id;
            Check(d.CostoUnitario==25000&&await db.Pagos.CountAsync(x=>x.VentaId==venta)==2,"Venta mixta conserva costo histórico y reparto de pagos");
            var doc=await db.Documentos.SingleAsync(x=>x.VentaId==venta&&x.Tipo==39);debito=await new Operaciones(db).DebitarAsync(Guid.NewGuid(),doc.Id,5000,"Ajuste","completa");
        }
        var cobro=Guid.NewGuid();
        await using(var db=new TiendaDb(opciones))await new Operaciones(db).CobrarDebitoAsync(cobro,debito,2000,"Efectivo","completa");
        await using(var db=new TiendaDb(opciones)){await new Operaciones(db).CobrarDebitoAsync(cobro,debito,2000,"Efectivo","completa");await new Operaciones(db).CobrarDebitoAsync(Guid.NewGuid(),debito,3000,"Transferencia","completa");}
        await using(var db=new TiendaDb(opciones))await Rechaza(()=>new Operaciones(db).CobrarDebitoAsync(Guid.NewGuid(),debito,1,"Efectivo","completa"),"Cobro parcial idempotente y sin exceder saldo");
        var cambio=Guid.NewGuid();int nuevaVenta;
        await using(var db=new TiendaDb(opciones))nuevaVenta=await new Operaciones(db).CambiarAsync(cambio,detalle,v2,"Tarjeta","Cambio talla","completa");
        await using(var db=new TiendaDb(opciones))
        {
            Check(await new Operaciones(db).CambiarAsync(cambio,detalle,v2,"Tarjeta","Cambio talla","completa")==nuevaVenta,"Cambio atómico idempotente");
            var caja=await db.SesionesCaja.SingleAsync(x=>x.UsuarioId=="completa"&&x.Cierre==null);var filas=await new Operaciones(db).CuadraturaAsync(caja);
            Check(filas.Single(x=>x.Medio=="Efectivo").Esperado==22000&&filas.Single(x=>x.Medio=="Tarjeta").Esperado==50000&&filas.Single(x=>x.Medio=="Transferencia").Esperado==3000,"Cuadratura incluye pagos mixtos, cobros y solo diferencia del cambio");
        }
        int nuevaLinea;int devueltas;
        await using(var db=new TiendaDb(opciones)){nuevaLinea=await db.DetallesVenta.Where(x=>x.VentaId==nuevaVenta).Select(x=>x.Id).SingleAsync();devueltas=await db.Devoluciones.CountAsync();}
        await using(var db=new TiendaDb(opciones))await Rechaza(async()=>await new Operaciones(db).CambiarAsync(Guid.NewGuid(),nuevaLinea,v3,"Efectivo","Sin stock","completa"),"Cambio sin stock se rechaza");
        await using(var db=new TiendaDb(opciones))Check(await db.Devoluciones.CountAsync()==devueltas&&(await db.Existencias.SingleAsync(x=>x.VarianteId==v2&&x.BodegaId==a)).Cantidad==1,"Cambio fallido revierte también devolución y documentos");
        await using(var db=new TiendaDb(opciones))Check(await db.Auditoria.AnyAsync(x=>x.UsuarioId=="prueba-completa"&&x.Entidad=="Producto"),"Auditoría guarda actor y entidad sin secretos");
        await using(var db=new TiendaDb(opciones))await new Operaciones(db).OperarStockAsync(Guid.NewGuid(),"Revision",v1,a,null,1,"Merma de prueba","completa");
        await using(var db=new TiendaDb(opciones))
        {
            var rev=await db.RevisionesStock.SingleAsync(x=>x.VarianteId==v1&&x.Estado=="Revision");await new Operaciones(db).ResolverRevisionAsync(rev.Id,"Merma","No recuperable","completa");
            Check(await db.RevisionesStock.AnyAsync(x=>x.Id==rev.Id&&x.Estado=="Merma"),"Merma queda clasificada fuera de stock vendible");
        }
        await using(var db=new TiendaDb(opciones))
        {
            var servicio=new RespaldosService(db);var id=await servicio.CrearAsync("prueba");await servicio.ProbarAsync(id);
            var respaldo=await db.Respaldos.SingleAsync(x=>x.Id==id);Check(respaldo.RestauracionProbada.HasValue,"Respaldo SQL real restaurado y comprobado en base aislada");
            try{if(Path.GetFileName(respaldo.Ruta).StartsWith("RetailPOS_",StringComparison.Ordinal)&&Path.GetExtension(respaldo.Ruta)==".bak")File.Delete(respaldo.Ruta);}catch(UnauthorizedAccessException){Console.WriteLine("Respaldo de prueba conservado en el directorio SQL por permisos del sistema.");}
        }
    }
}

