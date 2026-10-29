using Frontend.Models.Api.Responses;
namespace Frontend.Models.ViewModels.Usuarios;
public sealed record UsuarioEstadoViewModel(UsuarioResponse Usuario, bool Activar);
