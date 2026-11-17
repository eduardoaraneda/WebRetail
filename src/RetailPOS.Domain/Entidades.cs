using System.ComponentModel.DataAnnotations;

namespace RetailPOS.Domain;

public sealed class Producto
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Marca { get; set; } = "";
    [Required, MaxLength(150)] public string Modelo { get; set; } = "";
    public bool Activo { get; set; } = true;
    public List<Variante> Variantes { get; set; } = [];
}

public sealed class Variante
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    [Required, MaxLength(60)] public string Sku { get; set; } = "";
    [MaxLength(60)] public string? CodigoBarras { get; set; }
    [Required, MaxLength(60)] public string Color { get; set; } = "";
    [Required, MaxLength(15)] public string Talla { get; set; } = "";
    [Required, MaxLength(10)] public string SistemaTalla { get; set; } = "EU";
    [Range(1, 999999999)] public decimal Precio { get; set; }
    public bool Activo { get; set; } = true;
}

public sealed class Bodega
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string Nombre { get; set; } = "";
    [Required, MaxLength(100)] public string Tienda { get; set; } = "Principal";
}

public sealed class Existencia
{
    public int VarianteId { get; set; }
    public Variante Variante { get; set; } = null!;
    public int BodegaId { get; set; }
    public Bodega Bodega { get; set; } = null!;
    public int Cantidad { get; set; }
    [Timestamp] public byte[] Version { get; set; } = [];
    public int StockMinimo { get; set; }
    public decimal? CostoPromedio { get; set; }
}

public sealed class Movimiento
{
    public long Id { get; set; }
    public int VarianteId { get; set; }
    public Variante Variante { get; set; } = null!;
    public int BodegaId { get; set; }
    public Bodega Bodega { get; set; } = null!;
    public int Cantidad { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    [MaxLength(250)] public string Motivo { get; set; } = "";
    [MaxLength(450)] public string UsuarioId { get; set; } = "";
    public int? VentaId { get; set; }
}

public sealed class Cliente
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string Nombre { get; set; } = "";
    [MaxLength(12)] public string? Rut { get; set; }
    [MaxLength(150)] public string? Giro { get; set; }
    [MaxLength(200)] public string? Direccion { get; set; }
    [MaxLength(100)] public string? Comuna { get; set; }
}

public sealed class SesionCaja
{
    public DateOnly FechaOperativa { get; set; }
    public int Id { get; set; }
    [MaxLength(450)] public string UsuarioId { get; set; } = "";
    public DateTimeOffset Apertura { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Cierre { get; set; }
    public decimal FondoInicial { get; set; }
    public decimal? EfectivoEsperado { get; set; }
    public decimal? EfectivoContado { get; set; }
    [MaxLength(500)] public string? ObservacionCierre { get; set; }
    [MaxLength(450)] public string? RevisadoPor { get; set; }
    public DateTimeOffset? Revision { get; set; }
    public List<CuadraturaMedio> Cuadraturas { get; set; } = [];
}

public sealed class Venta
{
    [MaxLength(150)] public string NombreCliente { get; set; } = "Consumidor final";
    [MaxLength(12)] public string? RutCliente { get; set; }
    [MaxLength(150)] public string? GiroCliente { get; set; }
    [MaxLength(200)] public string? DireccionCliente { get; set; }
    [MaxLength(100)] public string? ComunaCliente { get; set; }
    public int Id { get; set; }
    public Guid SolicitudId { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public int BodegaId { get; set; }
    public Bodega Bodega { get; set; } = null!;
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public int SesionCajaId { get; set; }
    public SesionCaja SesionCaja { get; set; } = null!;
    [MaxLength(450)] public string UsuarioId { get; set; } = "";
    public decimal Total { get; set; }
    [MaxLength(20)] public string MedioPago { get; set; } = "Efectivo";
    public List<DetalleVenta> Detalles { get; set; } = [];
    public List<PagoVenta> Pagos { get; set; } = [];
}

public sealed class DetalleVenta
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;
    public int VarianteId { get; set; }
    public Variante Variante { get; set; } = null!;
    [MaxLength(300)] public string Descripcion { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PrecioLista { get; set; }
    public decimal? CostoUnitario { get; set; }
    [MaxLength(100)] public string MarcaVenta { get; set; } = "";
    public decimal DescuentoUnitario { get; set; }
    public int? OfertaId { get; set; }
    public Oferta? Oferta { get; set; }
    [MaxLength(120)] public string? OfertaNombre { get; set; }
}

// Documento interno de demostración: nunca representa un DTE autorizado por el SII.
public sealed class Documento
{
    public int Id { get; set; }
    public Guid SolicitudId { get; set; } = Guid.NewGuid();
    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;
    public int Tipo { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public int? ReferenciaId { get; set; }
    public Documento? Referencia { get; set; }
    [MaxLength(250)] public string Motivo { get; set; } = "";
    public decimal Total { get; set; }
    [MaxLength(30)] public string Estado { get; set; } = "Simulado";
    [MaxLength(450)] public string UsuarioId { get; set; } = "";
    public List<DetalleDocumento> Detalles { get; set; } = [];
}

public sealed class DetalleDocumento
{
    public int Id { get; set; }
    public int DocumentoId { get; set; }
    public Documento Documento { get; set; } = null!;
    [MaxLength(300)] public string Descripcion { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public sealed class Devolucion
{
    public int Id { get; set; }
    public Guid SolicitudId { get; set; }
    public int DetalleVentaId { get; set; }
    public DetalleVenta DetalleVenta { get; set; } = null!;
    public int Cantidad { get; set; }
    public bool ReingresarStock { get; set; }
    public int DocumentoId { get; set; }
    public Documento Documento { get; set; } = null!;
    public int SesionCajaId { get; set; }
    public SesionCaja SesionCaja { get; set; } = null!;
    public decimal Reembolso { get; set; }
    [MaxLength(20)] public string MedioReembolso { get; set; } = "";
    [MaxLength(450)] public string UsuarioId { get; set; } = "";
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
}

