using System.ComponentModel.DataAnnotations;

namespace RetailPOS.Domain;

public sealed class Marca
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Nombre { get; set; } = "";
    public bool Activa { get; set; } = true;
}

public sealed class TallaCalzado
{
    public int Id { get; set; }
    [Required, MaxLength(15)] public string Valor { get; set; } = "";
    [Required, MaxLength(10)] public string Sistema { get; set; } = "EU";
    public bool Activa { get; set; } = true;
}

public sealed class Oferta
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string Nombre { get; set; } = "";
    public int? MarcaId { get; set; }
    public Marca? Marca { get; set; }
    public int? TallaId { get; set; }
    public TallaCalzado? Talla { get; set; }
    public int? VarianteId { get; set; }
    public Variante? Variante { get; set; }
    [MaxLength(15)] public string Tipo { get; set; } = "Porcentaje";
    public decimal Valor { get; set; }
    public DateOnly Inicio { get; set; }
    public DateOnly Fin { get; set; }
    public bool Activa { get; set; } = true;
    [MaxLength(450)] public string UsuarioId { get; set; } = "";
    public DateTimeOffset Creacion { get; set; } = DateTimeOffset.UtcNow;
}

public sealed record PrecioCalculado(decimal Base, decimal Final, int? OfertaId, string? OfertaNombre)
{
    public decimal Descuento => Base - Final;
}

public static class MotorOfertas
{
    // Se aplica la mejor oferta; los filtros de una misma oferta se combinan con AND.
    public static PrecioCalculado Calcular(Variante variante, IEnumerable<Oferta> ofertas, DateOnly fecha)
    {
        var resultado = new PrecioCalculado(variante.Precio, variante.Precio, null, null);
        foreach (var oferta in ofertas.OrderBy(x => x.Id))
        {
            if (!oferta.Activa || fecha < oferta.Inicio || fecha > oferta.Fin) continue;
            if (oferta.MarcaId.HasValue && (oferta.Marca is not { Activa: true } || !string.Equals(oferta.Marca.Nombre, variante.Producto.Marca, StringComparison.OrdinalIgnoreCase))) continue;
            if (oferta.TallaId.HasValue && (oferta.Talla is not { Activa: true } || !string.Equals(oferta.Talla.Valor, variante.Talla, StringComparison.OrdinalIgnoreCase) || oferta.Talla.Sistema != variante.SistemaTalla)) continue;
            if (oferta.VarianteId.HasValue && oferta.VarianteId.Value != variante.Id) continue;
            if (oferta.Valor <= 0 || oferta.Tipo is not ("Porcentaje" or "Monto") || (oferta.Tipo == "Porcentaje" && oferta.Valor > 100)) continue;
            var descuento = oferta.Tipo == "Porcentaje" ? variante.Precio * oferta.Valor / 100 : oferta.Valor;
            var final = Math.Max(1, decimal.Round(variante.Precio - descuento, 0, MidpointRounding.AwayFromZero));
            if (final < resultado.Final) resultado = new(variante.Precio, final, oferta.Id, oferta.Nombre);
        }
        return resultado;
    }
}

public sealed class MovimientoCaja
{
    public long Id { get; set; }
    public Guid SolicitudId { get; set; }
    public int SesionCajaId { get; set; }
    public SesionCaja SesionCaja { get; set; } = null!;
    [MaxLength(10)] public string Tipo { get; set; } = "Ingreso";
    public decimal Importe { get; set; }
    [Required, MaxLength(250)] public string Motivo { get; set; } = "";
    [MaxLength(450)] public string UsuarioId { get; set; } = "";
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class CuadraturaMedio
{
    public int Id { get; set; }
    public int SesionCajaId { get; set; }
    public SesionCaja SesionCaja { get; set; } = null!;
    [MaxLength(20)] public string Medio { get; set; } = "";
    public decimal Ventas { get; set; }
    public decimal Reembolsos { get; set; }
    public decimal Fondo { get; set; }
    public decimal Ingresos { get; set; }
    public decimal Retiros { get; set; }
    public decimal Esperado { get; set; }
    public decimal Contado { get; set; }
}

