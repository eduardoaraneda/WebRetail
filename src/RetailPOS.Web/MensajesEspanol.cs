using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
namespace RetailPOS.Web;

public sealed class ValidacionEspanol : IValidationMetadataProvider, IDisplayMetadataProvider
{
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        var nombre=context.Key.Name;
        if(nombre is null)return;
        var nombres=new Dictionary<string,string>{{"Email","correo electrónico"},{"Password","contraseña"},{"Confirmar","confirmación de contraseña"},{"Rol","rol"},{"Sku","SKU"},{"CodigoBarras","código de barras"},{"Direccion","dirección"},{"Contado","efectivo contado"},{"Observacion","observación"}};
        context.DisplayMetadata.DisplayName=()=>nombres.GetValueOrDefault(nombre,nombre);
    }
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        foreach(var atributo in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
        {
            if(atributo.ErrorMessageResourceType is not null)continue;
            atributo.ErrorMessage=atributo switch
            {
                RequiredAttribute=>"El campo {0} es obligatorio.",
                EmailAddressAttribute=>"Ingresa un correo electrónico válido.",
                StringLengthAttribute=>"El campo {0} debe tener entre {2} y {1} caracteres.",
                MaxLengthAttribute=>"El campo {0} admite como máximo {1} caracteres.",
                MinLengthAttribute=>"El campo {0} requiere al menos {1} elementos o caracteres.",
                RangeAttribute=>"El campo {0} debe estar entre {1} y {2}.",
                CompareAttribute=>"La confirmación no coincide con la contraseña.",
                RegularExpressionAttribute=>"El formato del campo {0} no es válido.",
                _=>"Revisa el campo {0}."
            };
        }
    }
}
public sealed class IdentidadEspanol : IdentityErrorDescriber
{
    private static IdentityError Error(string codigo,string mensaje)=>new(){Code=codigo,Description=mensaje};
    public override IdentityError DefaultError()=>Error(nameof(DefaultError),"No se pudo completar la operación. Inténtalo de nuevo.");
    public override IdentityError ConcurrencyFailure()=>Error(nameof(ConcurrencyFailure),"La cuenta cambió durante la operación. Actualiza la página e inténtalo de nuevo.");
    public override IdentityError PasswordMismatch()=>Error(nameof(PasswordMismatch),"La contraseña no es correcta.");
    public override IdentityError InvalidToken()=>Error(nameof(InvalidToken),"El enlace no es válido o ya venció.");
    public override IdentityError LoginAlreadyAssociated()=>Error(nameof(LoginAlreadyAssociated),"Este acceso ya está asociado a otra cuenta.");
    public override IdentityError InvalidUserName(string? userName)=>Error(nameof(InvalidUserName),"El nombre de usuario no es válido. Utiliza tu correo electrónico.");
    public override IdentityError InvalidEmail(string? email)=>Error(nameof(InvalidEmail),"El correo electrónico no es válido.");
    public override IdentityError DuplicateUserName(string userName)=>Error(nameof(DuplicateUserName),"Ya existe una cuenta con ese correo electrónico.");
    public override IdentityError DuplicateEmail(string email)=>Error(nameof(DuplicateEmail),"Ya existe una cuenta con ese correo electrónico.");
    public override IdentityError InvalidRoleName(string? role)=>Error(nameof(InvalidRoleName),"El rol seleccionado no es válido.");
    public override IdentityError DuplicateRoleName(string role)=>Error(nameof(DuplicateRoleName),"Ese rol ya existe.");
    public override IdentityError UserAlreadyHasPassword()=>Error(nameof(UserAlreadyHasPassword),"Esta cuenta ya tiene una contraseña.");
    public override IdentityError UserLockoutNotEnabled()=>Error(nameof(UserLockoutNotEnabled),"El bloqueo de acceso no está habilitado para esta cuenta.");
    public override IdentityError UserAlreadyInRole(string role)=>Error(nameof(UserAlreadyInRole),"La cuenta ya tiene ese rol.");
    public override IdentityError UserNotInRole(string role)=>Error(nameof(UserNotInRole),"La cuenta no tiene el rol indicado.");
    public override IdentityError PasswordTooShort(int length)=>Error(nameof(PasswordTooShort),$"La contraseña debe tener al menos {length} caracteres.");
    public override IdentityError PasswordRequiresNonAlphanumeric()=>Error(nameof(PasswordRequiresNonAlphanumeric),"La contraseña debe incluir al menos un símbolo.");
    public override IdentityError PasswordRequiresDigit()=>Error(nameof(PasswordRequiresDigit),"La contraseña debe incluir al menos un número.");
    public override IdentityError PasswordRequiresLower()=>Error(nameof(PasswordRequiresLower),"La contraseña debe incluir al menos una letra minúscula.");
    public override IdentityError PasswordRequiresUpper()=>Error(nameof(PasswordRequiresUpper),"La contraseña debe incluir al menos una letra mayúscula.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)=>Error(nameof(PasswordRequiresUniqueChars),$"La contraseña debe incluir al menos {uniqueChars} caracteres distintos.");
}

