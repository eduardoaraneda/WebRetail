using Microsoft.EntityFrameworkCore;
using RetailPOS.Domain;

namespace RetailPOS.Infrastructure;

public sealed class Precios(TiendaDb db)
{
    public static DateOnly HoyChile => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Santiago")).DateTime);

    public Task<List<Oferta>> VigentesAsync(CancellationToken ct = default)
    {
        var hoy = HoyChile;
        return db.Ofertas.AsNoTracking().Include(x => x.Marca).Include(x => x.Talla).Where(x => x.Activa && x.Inicio <= hoy && x.Fin >= hoy).ToListAsync(ct);
    }
}

