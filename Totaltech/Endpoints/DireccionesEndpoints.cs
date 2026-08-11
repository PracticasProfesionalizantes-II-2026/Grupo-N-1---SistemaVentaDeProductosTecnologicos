using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class DireccionesEndpoints
    {
        public static void MapDireccionesEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/direcciones").WithTags("Direcciones");
            // obtener todas las direcciones
            group.MapGet("/", async (IDireccionesLogica logica) =>
            {
                var direcciones = await logica.ObtenerTodosAsync();
                return Results.Ok(direcciones);
            });
            // obtener una direccion por su id
            group.MapGet("/{id:int}", async (int id, IDireccionesLogica logica) =>
            {
                var direccion = await logica.ObtenerPorIdAsync(id);
                return direccion is null ? Results.NotFound() : Results.Ok(direccion);
            });
            // crear una nueva direccion
            group.MapPost("/", async (Direccion direccion, IDireccionesLogica logica) =>
            {
                var error = await logica.CrearAsync(direccion);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/direcciones/{direccion.IdDireccion}", direccion);
            });
            // actualizar una direccion existente
            group.MapPut("/{id:int}", async (int id, Direccion direccion, IDireccionesLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                direccion.IdDireccion = id;
                var error = await logica.ActualizarAsync(direccion);
                return error is null ? Results.Ok(direccion) : Results.BadRequest(error);
            });
            // eliminar una direccion
            group.MapDelete("/{id:int}", async (int id, IDireccionesLogica logica) =>
            {
                try
                {
                    var eliminado = await logica.EliminarAsync(id);
                    return eliminado ? Results.NoContent() : Results.NotFound();
                }
                catch (DbUpdateException)
                {
                    return Results.Conflict("No se puede eliminar porque hay datos relacionados.");
                }
            });
        }
    }
}
