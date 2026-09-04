using System.ComponentModel.DataAnnotations;

namespace Frontend.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresá tu email.")]
    [EmailAddress(ErrorMessage = "Ingresá un email válido.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ingresá tu nombre.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 60 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu apellido.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 60 caracteres.")]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu email.")]
    [EmailAddress(ErrorMessage = "Ingresá un email válido.")]
    [StringLength(120)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá un teléfono.")]
    [RegularExpression(@"^[0-9+()\-\s]{7,25}$", ErrorMessage = "Ingresá un teléfono válido.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Creá una contraseña.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Repetí la contraseña.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarContrasena { get; set; } = string.Empty;

    public bool AceptaTerminos { get; set; }
}
