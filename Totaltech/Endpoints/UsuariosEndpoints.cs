using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class UsuariosEndpoints
    {
        public static void MapUsuariosEndpoints(this WebApplication app)
        {
            // Estos endpoints traducen HTTP y delegan reglas de negocio a la capa de logica.
            var group = app.MapGroup("/usuarios").WithTags("Usuarios");

            group.MapGet("/", async (IUsuariosLogica logica) =>
            {
                var usuarios = await logica.ObtenerTodosAsync();
                return Results.Ok(usuarios.Select(usuario => usuario.ToResponse()));
            });

            group.MapGet("/{id:int}", async (int id, IUsuariosLogica logica) =>
            {
                var usuario = await logica.ObtenerPorIdAsync(id);
                return usuario is null ? Results.NotFound() : Results.Ok(usuario.ToResponse());
            });

            group.MapPost("/", async (CrearUsuarioRequest request, IUsuariosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearValidadoAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, usuario => Results.Created($"/usuarios/{usuario.IdUsuario}", usuario.ToResponse()));
                });
            });

            group.MapPut("/{id:int}", async (int id, ActualizarUsuarioRequest request, IUsuariosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarValidadoAsync(id, request.ToEntity(id));
                    return EndpointResults.FromResult(resultado, usuario => Results.Ok(usuario.ToResponse()));
                });
            });

            group.MapDelete("/{id:int}", async (int id, IUsuariosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.EliminarPorIdAsync(id);
                    return EndpointResults.FromResult(resultado, () => Results.NoContent());
                });
            });
        }
    }
}
