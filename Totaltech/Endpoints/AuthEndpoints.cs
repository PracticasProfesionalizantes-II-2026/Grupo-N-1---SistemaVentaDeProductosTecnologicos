using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

namespace Totaltech.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/auth").WithTags("Auth");

            // login--------------------------------
            group.MapPost("/login", async (LoginDto request, IUsuariosLogica logica, IJwtTokenService tokens) =>
            {
                if (!await logica.ExisteEmailAsync(request.Email))
                {
                    return Results.NotFound(new
                    {
                        codigo = "usuario_no_registrado",
                        mensaje = "No existe una cuenta asociada a ese email."
                    });
                }

                var usuario = await logica.LoginAsync(request);
                if (usuario is null)
                {
                    return Results.Unauthorized();
                }

                var token = tokens.Crear(usuario);
                var respuesta = CrearRespuesta(usuario);
                return Results.Ok(new LoginResponse
                {
                    IdUsuario = respuesta.IdUsuario,
                    Nombre = respuesta.Nombre,
                    Apellido = respuesta.Apellido,
                    Email = respuesta.Email,
                    Telefono = respuesta.Telefono,
                    FechaRegistro = respuesta.FechaRegistro,
                    Rol = respuesta.Rol,
                    AccessToken = token.AccessToken,
                    ExpiresAtUtc = token.ExpiresAtUtc
                });
            }).AllowAnonymous();

            // registrar un nuevo usuario--------------------------------
            group.MapPost("/registro", async (UsuarioRequest request, IUsuariosLogica logica) =>
            {
                var usuario = request.ToEntity();
                var error = await logica.RegistrarAsync(usuario);
                if (error is not null)
                {
                    return error.StartsWith("Ya existe") ? Results.Conflict(error) : Results.BadRequest(error);
                }

                return Results.Created($"/usuarios/{usuario.IdUsuario}", CrearRespuesta(usuario));
            }).AllowAnonymous();

            // recuperar contraseña--------------------------------
            group.MapPost("/recuperar-contrasena", async (RecuperarContrasenaDto request, IUsuariosLogica logica) =>
            {
                await logica.RecuperarContrasenaAsync(request);
                return Results.Ok(new { mensaje = "Si el email existe, se registrara una solicitud de recuperacion." });
            }).AllowAnonymous();
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
