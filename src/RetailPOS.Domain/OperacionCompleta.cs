using System.ComponentModel.DataAnnotations;
namespace RetailPOS.Domain;

public sealed class Proveedor
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string Nombre { get; set; } = "";
    [MaxLength(12)] public string? Rut { get; set; }
    [MaxLength(200)] public string? Contacto { get; set; }
    [MaxLength(200)] public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;
}
public sealed class Compra
{
    public int Id { get; set; }
    public Guid SolicitudId { get; set; }
    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public int BodegaId { get; set; }
    public Bodega Bodega { get; set; } = null!;
    [MaxLength(80)] public string Referencia { get; set; } = "";
    public decimal Total { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    [MaxLength(450)] public string UsuarioId { get; set; } = "";
    public List<DetalleCompra> Detalles { get; set; } = [];
}
public sealed class DetalleCompra
{
    public int Id { get; set; }
    public int CompraId { get; set; }
    public Compra Compra { get; set; } = null!;
    public int VarianteId { get; set; }
    public Variante Variante { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
}
public sealed class PagoVenta
{
    public int Id { get; set; }
    public Guid SolicitudId { get; set; } = Guid.NewGuid();
    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;
    public int? DocumentoId { get; set; }
    public Documento? Documento { get; set; }
    public int SesionCajaId { get; set; }
    public SesionCaja SesionCaja { get; set; } = null!;
    [MaxLength(20)] public string Medio { get; set; } = "Efectivo";
    public decimal Importe { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
}
public sealed class ReembolsoPago
{
    public int Id { get; set; }
    public int DevolucionId { get; set; }
    public Devolucion Devolucion { get; set; } = null!;
    public int SesionCajaId { get; set; }
    public SesionCaja SesionCaja { get; set; } = null!;
    [MaxLength(20)] public string Medio { get; set; } = "Efectivo";
    public decimal Importe { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
}
public sealed class OperacionStock
{
    public int Id { get; set; }
    public Guid SolicitudId { get; set; }
    [MaxLength(20)] public string Tipo { get; set; } = "Traspaso";
    public int VarianteId { get; set; }
    public Variante Variante { get; set; } = null!;
    public int BodegaId { get; set; }
    public Bodega Bodega { get; set; } = null!;
    public int? DestinoId { get; set; }
    public Bodega? Destino { get; set; }
    public int Cantidad { get; set; }
    public int? ConteoFisico { get; set; }
    [MaxLength(250)] public string Motivo { get; set; } = "";
    [MaxLength(450)] public string UsuarioId { get; set; } = "";
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
}
public sealed class RevisionStock
{
    public int Id { get; set; }
    public int VarianteId { get; set; }
    public Variante Variante { get; set; } = null!;
    public int BodegaId { get; set; }
    public Bodega Bodega { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal? CostoUnitario { get; set; }
    public int? DevolucionId { get; set; }
    public Devolucion? Devolucion { get; set; }
    [MaxLength(20)] public string Estado { get; set; } = "Revision";
    [MaxLength(250)] public string Motivo { get; set; } = "";
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Resolucion { get; set; }
    [MaxLength(450)] public string UsuarioId { get; set; } = "";
}
public sealed class Cambio
{
    public int Id { get; set; }
    public Guid SolicitudId { get; set; }
    public int DevolucionId { get; set; }
    public Devolucion Devolucion { get; set; } = null!;
    public int VentaNuevaId { get; set; }
    public Venta VentaNueva { get; set; } = null!;
    public decimal Diferencia { get; set; }
    [MaxLength(450)] public string UsuarioId { get; set; } = "";
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
}
public sealed class EventoAuditoria
{
    public long Id { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    [MaxLength(450)] public string UsuarioId { get; set; } = "sistema";
    [MaxLength(100)] public string Entidad { get; set; } = "";
    [MaxLength(100)] public string Registro { get; set; } = "";
    [MaxLength(50)] public string Accion { get; set; } = "";
    [MaxLength(2000)] public string Campos { get; set; } = "";
}
public sealed class Respaldo
{
    public int Id { get; set; }
    [MaxLength(500)] public string Ruta { get; set; } = "";
    [MaxLength(450)] public string UsuarioId { get; set; } = "";
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RestauracionProbada { get; set; }
    [MaxLength(100)] public string Estado { get; set; } = "Verificado";
}

