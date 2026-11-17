using Microsoft.AspNetCore.Identity;
namespace RetailPOS.Infrastructure;
public sealed class UsuarioTienda : IdentityUser
{
    public bool Activo { get; set; } = true;
}

