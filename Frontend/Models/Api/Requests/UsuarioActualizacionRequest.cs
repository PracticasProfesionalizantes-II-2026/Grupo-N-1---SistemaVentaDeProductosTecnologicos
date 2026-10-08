namespace Frontend.Models.Api.Requests;

public sealed record UsuarioActualizacionRequest(string Nombre, string Apellido, string Email, string Telefono, int Rol);
