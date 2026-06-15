using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class CrudEndpointExtensions
    {
        public static RouteGroupBuilder MapCrud<TEntidad, TLogica>(
            this WebApplication app,
            string ruta,
            string tag,
            Func<TEntidad, int> obtenerId)
            where TEntidad : class
            where TLogica : ILogica<TEntidad>
        {
            var group = app.MapGroup(ruta).WithTags(tag);

            group.MapGet("/", async (TLogica logica) =>
            {
                var entidades = await logica.ObtenerTodosAsync();
                return Results.Ok(entidades);
            });

            group.MapGet("/{id:int}", async (int id, TLogica logica) =>
            {
                var entidad = await logica.ObtenerPorIdAsync(id);
                return entidad is null ? Results.NotFound() : Results.Ok(entidad);
            });

            group.MapPost("/", async (TEntidad entidad, TLogica logica) =>
            {
                await logica.CrearAsync(entidad);
                return Results.Created($"{ruta}/{obtenerId(entidad)}", entidad);
            });

            group.MapPut("/{id:int}", async (int id, TEntidad entidad, TLogica logica) =>
            {
                if (id != obtenerId(entidad))
                {
                    return Results.BadRequest("El id de la URL no coincide con el id del body.");
                }

                var existente = await logica.ObtenerPorIdAsync(id);

                if (existente is null)
                {
                    return Results.NotFound();
                }

                await logica.ActualizarAsync(entidad);
                return Results.NoContent();
            });

            group.MapDelete("/{id:int}", async (int id, TLogica logica) =>
            {
                var entidad = await logica.ObtenerPorIdAsync(id);

                if (entidad is null)
                {
                    return Results.NotFound();
                }

                await logica.EliminarAsync(entidad);
                return Results.NoContent();
            });

            return group;
        }
    }
}
