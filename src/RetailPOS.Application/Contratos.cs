using System.ComponentModel.DataAnnotations;
using RetailPOS.Domain;

namespace RetailPOS.Application;

public sealed class ReglaNegocioException(string mensaje) : Exception(mensaje);

public sealed class LineaSolicitud
{
    [Range(1, int.MaxValue)] public int VarianteId { get; set; }
    [Range(1, 10000)] public int Cantidad { get; set; }
}

public sealed class SolicitudVenta
{
    public Guid SolicitudId { get; set; } = Guid.NewGuid();
    [Range(1, int.MaxValue)] public int BodegaId { get; set; }
    public int? ClienteId { get; set; }
    public int TipoDocumento { get; set; } = 39;
    [Required] public string MedioPago { get; set; } = "Efectivo";
    [MinLength(1), MaxLength(100)] public List<LineaSolicitud> Lineas { get; set; } = [];
    [MaxLength(4)] public List<PagoSolicitud> Pagos { get; set; } = [];
}

public sealed class PagoSolicitud
{
    [Required] public string Medio { get; set; } = "Efectivo";
    [Range(1,999999999)] public decimal Importe { get; set; }
}

public sealed class SolicitudCompra
{
    public Guid SolicitudId { get; set; } = Guid.NewGuid();
    [Range(1,int.MaxValue)] public int ProveedorId { get; set; }
    [Range(1,int.MaxValue)] public int BodegaId { get; set; }
    [Required,MaxLength(80)] public string Referencia { get; set; } = "";
    [MinLength(1),MaxLength(100)] public List<LineaCompra> Lineas { get; set; } = [];
}
public sealed class LineaCompra
{
    [Range(1,int.MaxValue)] public int VarianteId { get; set; }
    [Range(1,100000)] public int Cantidad { get; set; }
    [Range(0,999999999)] public decimal CostoUnitario { get; set; }
}

public interface IOperaciones
{
    Task<int> VenderAsync(SolicitudVenta solicitud, string usuarioId, CancellationToken ct = default);
    Task MoverStockAsync(int varianteId, int bodegaId, int cantidad, string motivo, string usuarioId, CancellationToken ct = default);
    Task AbrirCajaAsync(decimal fondo, string usuarioId, CancellationToken ct = default);
    Task CerrarCajaAsync(decimal contado, string usuarioId, CancellationToken ct = default);
    Task CerrarCajaDetalladaAsync(SolicitudCuadratura solicitud, string usuarioId, CancellationToken ct = default);
    Task MovimientoCajaAsync(Guid solicitudId, string tipo, decimal importe, string motivo, string usuarioId, CancellationToken ct = default);
    Task<List<CuadraturaMedio>> CuadraturaAsync(SesionCaja caja, CancellationToken ct = default);
    Task<ResumenCuadratura> ResumenCajaAsync(SesionCaja caja, CancellationToken ct = default);
    Task<ResumenCuadratura> ResumenDiaAsync(DateOnly fecha, string? usuarioId, CancellationToken ct = default);
    Task<int> CrearDocumentoAsync(int ventaId, int tipo, int? referenciaId, string motivo, string usuarioId, CancellationToken ct = default);
    Task<int> DevolverAsync(Guid solicitudId, int detalleVentaId, int cantidad, bool reingresarStock, string motivo, string usuarioId, CancellationToken ct = default);
    Task<int> DebitarAsync(Guid solicitudId, int referenciaId, decimal importe, string motivo, string usuarioId, CancellationToken ct = default);
}

public sealed class SolicitudCuadratura
{
    [Required, Range(0,999999999)] public decimal? Contado { get; set; }
    [Required, Range(-999999999,999999999)] public decimal? Tarjeta { get; set; }
    [Required, Range(-999999999,999999999)] public decimal? Transferencia { get; set; }
    [Required, Range(-999999999,999999999)] public decimal? Otros { get; set; }=0;
    [MaxLength(500)] public string? Observacion { get; set; }
}

