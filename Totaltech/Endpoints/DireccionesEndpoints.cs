using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class DireccionesEndpoints
    {
        public static void MapDireccionesEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/direcciones").WithTags("Direcciones");

            group.MapGet("/", async (IDireccionesLogica logica) =>
            {
                var direcciones = await logica.ObtenerTodosAsync();
                return Results.Ok(direcciones);
            });

            group.MapGet("/{id:int}", async (int id, IDireccionesLogica logica) =>
            {
                var direccion = await logica.ObtenerPorIdAsync(id);
                return direccion is null ? Results.NotFound() : Results.Ok(direccion);
            });

            group.MapPost("/", async (Direccion direccion, IDireccionesLogica logica) =>
            {
                await logica.CrearAsync(direccion);
                return Results.Created($"/direcciones/{direccion.IdDireccion}", direccion);
            });

            group.MapPut("/{id:int}", async (int id, Direccion direccion, IDireccionesLogica logica) =>
            {
                if (id != direccion.IdDireccion)
                {
                    return Results.BadRequest("El id de la URL no coincide con el id del body.");
                }

                var existente = await logica.ObtenerPorIdAsync(id);
                if (existente is null)
                {
                    return Results.NotFound();
                }

                await logica.ActualizarAsync(direccion);
                return Results.NoContent();
            });

            group.MapDelete("/{id:int}", async (int id, IDireccionesLogica logica) =>
            {
                var direccion = await logica.ObtenerPorIdAsync(id);
                if (direccion is null)
                {
                    return Results.NotFound();
                }

                await logica.EliminarAsync(direccion);
                return Results.NoContent();
            });
        }
    }
}
