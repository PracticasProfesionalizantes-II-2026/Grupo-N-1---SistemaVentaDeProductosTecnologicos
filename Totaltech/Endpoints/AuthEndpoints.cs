using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            // Auth mantiene un flujo simple: registra/login sin exponer contrasenas.
            var group = app.MapGroup("/auth").WithTags("Auth");

            group.MapPost("/login", async (LoginDto request, IUsuariosLogica logica) =>
            {
                var usuario = await logica.LoginAsync(request);
                return usuario is null ? Results.Unauthorized() : Results.Ok(usuario.ToResponse());
            });

            group.MapPost("/registro", async (CrearUsuarioRequest request, IUsuariosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.RegistrarAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, usuario => Results.Created($"/usuarios/{usuario.IdUsuario}", usuario.ToResponse()));
                });
            });

            group.MapPost("/recuperar-contrasena", async (RecuperarContrasenaDto request, IUsuariosLogica logica) =>
            {
                await logica.RecuperarContrasenaAsync(request);
                return Results.Ok(new { mensaje = "Si el email existe, se registrara una solicitud de recuperacion." });
            });
        }
    }
}
