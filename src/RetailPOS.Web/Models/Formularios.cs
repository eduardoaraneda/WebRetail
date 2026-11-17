using System.ComponentModel.DataAnnotations;

namespace RetailPOS.Web.Models;

public sealed class RegistroForm
{
    [Required,EmailAddress,MaxLength(200)] public string Email { get; set; }="";
    [Required,DataType(DataType.Password),StringLength(100,MinimumLength=12)] public string Password { get; set; }="";
    [Required,DataType(DataType.Password),Compare(nameof(Password))] public string Confirmar { get; set; }="";
}

public sealed class RecuperacionForm
{
    [Required,EmailAddress,MaxLength(200)] public string Email { get; set; } = "";
}
public sealed class RestablecerForm
{
    [Required,EmailAddress,MaxLength(200)] public string Email { get; set; } = "";
    [Required,MaxLength(8192)] public string Token { get; set; } = "";
    [Required,DataType(DataType.Password),StringLength(100,MinimumLength=12)] public string Password { get; set; } = "";
    [Required,DataType(DataType.Password),Compare(nameof(Password))] public string Confirmar { get; set; } = "";
}

public sealed class AccesoForm
{
    [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 12)] public string Password { get; set; } = "";
}
public sealed class UsuarioForm
{
    [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 12)] public string Password { get; set; } = "";
    [Required, RegularExpression("^(Administrador|Vendedor|Bodega)$")] public string Rol { get; set; } = "Vendedor";
}
public sealed class VarianteForm
{
    [Range(1,int.MaxValue)] public int ProductoId { get; set; }
    [Required, MaxLength(60)] public string Sku { get; set; } = "";
    [MaxLength(60)] public string? CodigoBarras { get; set; }
    [Required, MaxLength(60)] public string Color { get; set; } = "";
    [Range(1,int.MaxValue)] public int TallaId { get; set; }
    [Range(1,999999999)] public decimal Precio { get; set; }
}

