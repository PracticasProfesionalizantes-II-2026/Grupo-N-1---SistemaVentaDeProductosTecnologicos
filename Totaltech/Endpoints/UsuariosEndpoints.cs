using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class UsuariosEndpoints
    {
        public static void MapUsuariosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/usuarios").WithTags("Usuarios");

            group.MapGet("/", async (IUsuariosLogica logica) =>
            {
                var usuarios = await logica.ObtenerTodosAsync();
                return Results.Ok(usuarios);
            });

            group.MapGet("/{id:int}", async (int id, IUsuariosLogica logica) =>
            {
                var usuario = await logica.ObtenerPorIdAsync(id);
                return usuario is null ? Results.NotFound() : Results.Ok(usuario);
            });

            group.MapPost("/", async (Usuario usuario, IUsuariosLogica logica) =>
            {
                await logica.CrearAsync(usuario);
                return Results.Created($"/usuarios/{usuario.IdUsuario}", usuario);
            });

            group.MapPut("/{id:int}", async (int id, Usuario usuario, IUsuariosLogica logica) =>
            {
                if (id != usuario.IdUsuario)
                {
                    return Results.BadRequest("El id de la URL no coincide con el id del body.");
                }

                var existente = await logica.ObtenerPorIdAsync(id);
                if (existente is null)
                {
                    return Results.NotFound();
                }

                await logica.ActualizarAsync(usuario);
                return Results.NoContent();
            });

            group.MapDelete("/{id:int}", async (int id, IUsuariosLogica logica) =>
            {
                var usuario = await logica.ObtenerPorIdAsync(id);
                if (usuario is null)
                {
                    return Results.NotFound();
                }

                await logica.EliminarAsync(usuario);
                return Results.NoContent();
            });
        }
    }
}
