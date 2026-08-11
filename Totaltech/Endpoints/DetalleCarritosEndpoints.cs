using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class DetalleCarritosEndpoints
    {
        public static void MapDetalleCarritosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/detallecarritos").WithTags("DetalleCarritos");

            group.MapGet("/", async (IDetalleCarritosLogica logica) =>
            {
                var detalles = await logica.ObtenerTodosAsync();
                return Results.Ok(detalles);
            });

            group.MapGet("/{id:int}", async (int id, IDetalleCarritosLogica logica) =>
            {
                var detalle = await logica.ObtenerPorIdAsync(id);
                return detalle is null ? Results.NotFound() : Results.Ok(detalle);
            });

            group.MapGet("/carrito/{idCarrito:int}", async (int idCarrito, IDetalleCarritosLogica logica) =>
            {
                var detalles = await logica.ObtenerPorCarritoAsync(idCarrito);
                return Results.Ok(detalles);
            });

            group.MapPost("/", async (DetalleCarritoRequest request, IDetalleCarritosLogica logica) =>
            {
                var detalle = request.ToEntity();
                var error = await logica.CrearAsync(detalle);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/detallecarritos/{detalle.IdDetalleCarrito}", detalle);
            });

            group.MapPut("/{id:int}", async (int id, DetalleCarritoRequest request, IDetalleCarritosLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                var detalle = request.ToEntity();
                detalle.IdDetalleCarrito = id;
                var error = await logica.ActualizarAsync(detalle);
                return error is null ? Results.Ok(detalle) : Results.BadRequest(error);
            });

            group.MapDelete("/{id:int}", async (int id, IDetalleCarritosLogica logica) =>
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
