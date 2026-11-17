using RetailPOS.Application;
namespace RetailPOS.Web;
public static class Validaciones
{
    public static string? Rut(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        var rut = valor.Replace(".", "").Replace("-", "").Trim().ToUpperInvariant();
        if (rut.Length is < 2 or > 9 || !int.TryParse(rut[..^1], out var numero) || numero <= 0) throw new ReglaNegocioException("RUT inválido.");
        var suma = 0; var factor = 2;
        for (var n = numero; n > 0; n /= 10) { suma += n % 10 * factor; factor = factor == 7 ? 2 : factor + 1; }
        var digito = 11 - suma % 11; var esperado = digito == 11 ? "0" : digito == 10 ? "K" : digito.ToString();
        if (rut[^1].ToString() != esperado) throw new ReglaNegocioException("Dígito verificador del RUT inválido.");
        return $"{numero}-{esperado}";
    }
}

