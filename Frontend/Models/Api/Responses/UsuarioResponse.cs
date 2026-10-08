namespace Frontend.Models.Api.Responses;

public sealed class UsuarioResponse
{
    public int IdUsuario { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Apellido { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public DateTime FechaRegistro { get; init; }
    public int Rol { get; init; }
    public bool Activo { get; init; }
}
