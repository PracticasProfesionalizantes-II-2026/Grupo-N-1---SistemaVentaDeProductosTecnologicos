using System.ComponentModel.DataAnnotations;
namespace Frontend.Models.ViewModels.Usuarios;

public sealed class UsuarioEdicionViewModel
{
    public int IdUsuario { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;
    [Required(ErrorMessage = "El apellido es obligatorio.")]
    public string Apellido { get; set; } = string.Empty;
    [Required(ErrorMessage = "El email es obligatorio."), EmailAddress(ErrorMessage = "Ingresá un email válido."), StringLength(256)]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "El teléfono es obligatorio."), Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;
    [Range(0, 1, ErrorMessage = "Seleccioná un rol válido.")]
    public int Rol { get; set; }
}
