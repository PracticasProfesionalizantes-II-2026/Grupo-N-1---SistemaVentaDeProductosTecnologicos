using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

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

            group.MapPost("/", async (DireccionRequest request, IDireccionesLogica logica) =>
            {
                var direccion = request.ToEntity();
                var error = await logica.CrearAsync(direccion);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/direcciones/{direccion.IdDireccion}", direccion);
            });

            group.MapPut("/{id:int}", async (int id, DireccionRequest request, IDireccionesLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                var direccion = request.ToEntity();
                direccion.IdDireccion = id;
                var error = await logica.ActualizarAsync(direccion);
                return error is null ? Results.Ok(direccion) : Results.BadRequest(error);
            });

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
