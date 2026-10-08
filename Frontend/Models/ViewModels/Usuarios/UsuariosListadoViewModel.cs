using Frontend.Models.Api.Responses;
namespace Frontend.Models.ViewModels.Usuarios;

public sealed class UsuariosListadoViewModel
{
    public List<UsuarioResponse> Usuarios { get; init; } = [];
    public string? Error { get; init; }
}
