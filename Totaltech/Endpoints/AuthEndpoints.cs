using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/auth").WithTags("Auth");

            group.MapPost("/login", async (LoginDto request, IUsuariosLogica logica) =>
            {
                var usuario = await logica.LoginAsync(request);
                return usuario is null ? Results.Unauthorized() : Results.Ok(CrearRespuesta(usuario));
            });

            group.MapPost("/registro", async (UsuarioRequest request, IUsuariosLogica logica) =>
            {
                var usuario = request.ToEntity();
                var error = await logica.RegistrarAsync(usuario);
                if (error is not null)
                {
                    return error.StartsWith("Ya existe") ? Results.Conflict(error) : Results.BadRequest(error);
                }

                return Results.Created($"/usuarios/{usuario.IdUsuario}", CrearRespuesta(usuario));
            });

            group.MapPost("/recuperar-contrasena", async (RecuperarContrasenaDto request, IUsuariosLogica logica) =>
            {
                await logica.RecuperarContrasenaAsync(request);
                return Results.Ok(new { mensaje = "Si el email existe, se registrara una solicitud de recuperacion." });
            });
        }

        private static UsuarioResponse CrearRespuesta(Usuario usuario)
        {
            return new UsuarioResponse
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                FechaRegistro = usuario.FechaRegistro,
                Rol = usuario.Rol
            };
        }
    }
}
