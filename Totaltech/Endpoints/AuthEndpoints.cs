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

                if (usuario is null)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(usuario);
            });

            group.MapPost("/registro", async (Usuario usuario, IUsuariosLogica logica) =>
            {
                var registrado = await logica.RegistrarAsync(usuario);

                if (registrado is null)
                {
                    return Results.Conflict("Ya existe un usuario registrado con ese email.");
                }

                return Results.Created($"/usuarios/{registrado.IdUsuario}", registrado);
            });

            group.MapPost("/recuperar-contrasena", async (RecuperarContrasenaDto request, IUsuariosLogica logica) =>
            {
                var existe = await logica.RecuperarContrasenaAsync(request);

                if (!existe)
                {
                    return Results.NotFound("No existe un usuario registrado con ese email.");
                }

                return Results.Ok(new { mensaje = "Solicitud de recuperacion registrada." });
            });
        }
    }
}
