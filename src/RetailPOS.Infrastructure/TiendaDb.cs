using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RetailPOS.Domain;

namespace RetailPOS.Infrastructure;

public sealed class TiendaDb(DbContextOptions<TiendaDb> options) : IdentityDbContext<UsuarioTienda>(options)
{
    public string ActorId { get; set; } = "sistema";
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<PagoVenta> Pagos => Set<PagoVenta>();
    public DbSet<ReembolsoPago> Reembolsos => Set<ReembolsoPago>();
    public DbSet<OperacionStock> OperacionesStock => Set<OperacionStock>();
    public DbSet<RevisionStock> RevisionesStock => Set<RevisionStock>();
    public DbSet<Cambio> Cambios => Set<Cambio>();
    public DbSet<EventoAuditoria> Auditoria => Set<EventoAuditoria>();
    public DbSet<Respaldo> Respaldos => Set<Respaldo>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Variante> Variantes => Set<Variante>();
    public DbSet<Bodega> Bodegas => Set<Bodega>();
    public DbSet<Existencia> Existencias => Set<Existencia>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<SesionCaja> SesionesCaja => Set<SesionCaja>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<Devolucion> Devoluciones => Set<Devolucion>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<TallaCalzado> Tallas => Set<TallaCalzado>();
    public DbSet<Oferta> Ofertas => Set<Oferta>();
    public DbSet<MovimientoCaja> MovimientosCaja => Set<MovimientoCaja>();
    public DbSet<CuadraturaMedio> Cuadraturas => Set<CuadraturaMedio>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<UsuarioTienda>().Property(x => x.Activo).HasDefaultValue(true);
        b.Entity<Compra>().HasIndex(x => x.SolicitudId).IsUnique();
        b.Entity<Compra>().HasIndex(x => new { x.ProveedorId, x.Referencia }).IsUnique();
        b.Entity<PagoVenta>().HasIndex(x => x.SolicitudId).IsUnique();
        b.Entity<PagoVenta>().ToTable(t => t.HasCheckConstraint("CK_Pago_Importe", "[Importe]>0 AND [Medio] IN ('Efectivo','Tarjeta','Transferencia','Otros','SaldoCambio')"));
        b.Entity<ReembolsoPago>().ToTable(t => t.HasCheckConstraint("CK_Reembolso_Importe", "[Importe]>0 AND [Medio] IN ('Efectivo','Tarjeta','Transferencia','Otros')"));
        b.Entity<OperacionStock>().HasIndex(x => x.SolicitudId).IsUnique();
        b.Entity<OperacionStock>().HasOne(x => x.Destino).WithMany().HasForeignKey(x => x.DestinoId);
        b.Entity<RevisionStock>().HasIndex(x => x.DevolucionId).IsUnique().HasFilter("[DevolucionId] IS NOT NULL");
        b.Entity<Cambio>().HasIndex(x => x.SolicitudId).IsUnique();
        b.Entity<Marca>().HasAlternateKey(x => x.Nombre);
        b.Entity<TallaCalzado>().HasAlternateKey(x => new { x.Valor, x.Sistema });
        b.Entity<Producto>().HasOne<Marca>().WithMany().HasForeignKey(x => x.Marca).HasPrincipalKey(x => x.Nombre);
        b.Entity<Variante>().HasOne<TallaCalzado>().WithMany().HasForeignKey(x => new { x.Talla, x.SistemaTalla }).HasPrincipalKey(x => new { x.Valor, x.Sistema });
        b.Entity<MovimientoCaja>().HasIndex(x => x.SolicitudId).IsUnique();
        b.Entity<CuadraturaMedio>().HasIndex(x => new { x.SesionCajaId, x.Medio }).IsUnique();
        b.Entity<Oferta>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Oferta_Filtro", "[MarcaId] IS NOT NULL OR [TallaId] IS NOT NULL OR [VarianteId] IS NOT NULL");
            t.HasCheckConstraint("CK_Oferta_Valor", "([Tipo]='Porcentaje' AND [Valor]>0 AND [Valor]<=100) OR ([Tipo]='Monto' AND [Valor]>0 AND [Valor]=FLOOR([Valor]))");
            t.HasCheckConstraint("CK_Oferta_Fechas", "[Fin]>=[Inicio]");
        });
        b.Entity<MovimientoCaja>().ToTable(t => t.HasCheckConstraint("CK_MovimientoCaja_Importe", "[Importe]>0 AND [Tipo] IN ('Ingreso','Retiro')"));
        b.Entity<Variante>().HasIndex(x => x.Sku).IsUnique();
        b.Entity<Variante>().HasIndex(x => x.CodigoBarras).IsUnique().HasFilter("[CodigoBarras] IS NOT NULL");
        b.Entity<Variante>().HasIndex(x => new { x.ProductoId, x.Color, x.Talla, x.SistemaTalla }).IsUnique();
        b.Entity<Bodega>().HasIndex(x => x.Nombre).IsUnique();
        b.Entity<Existencia>().HasKey(x => new { x.VarianteId, x.BodegaId });
        b.Entity<Existencia>().ToTable(t => t.HasCheckConstraint("CK_Existencia_Cantidad", "[Cantidad] >= 0"));
        b.Entity<Venta>().HasIndex(x => x.SolicitudId).IsUnique();
        b.Entity<SesionCaja>().HasIndex(x => x.UsuarioId).IsUnique().HasFilter("[Cierre] IS NULL");
        b.Entity<SesionCaja>().HasIndex(x=>new{x.FechaOperativa,x.UsuarioId});
        b.Entity<Documento>().HasOne(x => x.Referencia).WithMany().HasForeignKey(x => x.ReferenciaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Documento>().HasIndex(x => new { x.VentaId, x.Tipo }).IsUnique().HasFilter("[Tipo] IN (33, 39, 52)");
        b.Entity<Documento>().HasIndex(x => x.SolicitudId).IsUnique();
        b.Entity<Devolucion>().HasIndex(x => x.SolicitudId).IsUnique();
        b.Entity<Devolucion>().HasIndex(x => x.DocumentoId).IsUnique();
        foreach (var entity in b.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties().Where(x => x.ClrType == typeof(decimal) || x.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
            foreach (var fk in entity.GetForeignKeys()) fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entradas = ChangeTracker.Entries().Where(x => x.Entity is not EventoAuditoria && x.Entity.GetType().Namespace == "RetailPOS.Domain" && x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(x => new { Entrada = x, Accion = x.State.ToString(), Campos = string.Join(",", x.Properties.Where(p => x.State != EntityState.Modified || p.IsModified).Select(p => p.Metadata.Name)) }).ToList();
        await using var tx = Database.CurrentTransaction is null ? await Database.BeginTransactionAsync(cancellationToken) : null;
        var resultado = await base.SaveChangesAsync(cancellationToken);
        foreach (var e in entradas)
            Auditoria.Add(new EventoAuditoria { UsuarioId = ActorId, Entidad = e.Entrada.Metadata.ClrType.Name, Registro = string.Join("/", e.Entrada.Properties.Where(p => p.Metadata.IsPrimaryKey()).Select(p => p.CurrentValue)), Accion = e.Accion, Campos = e.Campos[..Math.Min(2000,e.Campos.Length)] });
        if (entradas.Count > 0) await base.SaveChangesAsync(cancellationToken);
        if (tx is not null) await tx.CommitAsync(cancellationToken);
        return resultado;
    }
}


