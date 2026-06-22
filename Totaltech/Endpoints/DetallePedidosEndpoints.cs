using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class DetallePedidosEndpoints
    {
        public static void MapDetallePedidosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/detallepedidos").WithTags("DetallePedidos");

            group.MapGet("/", async (IDetallePedidosLogica logica) =>
            {
                var detalles = await logica.ObtenerTodosAsync();
                return Results.Ok(detalles);
            });

            group.MapGet("/{id:int}", async (int id, IDetallePedidosLogica logica) =>
            {
                var detalle = await logica.ObtenerPorIdAsync(id);
                return detalle is null ? Results.NotFound() : Results.Ok(detalle);
            });

            group.MapPost("/", async (DetallePedido detalle, IDetallePedidosLogica logica) =>
            {
                var error = await logica.CrearAsync(detalle);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/detallepedidos/{detalle.IdDetallePedido}", detalle);
            });

            group.MapPut("/{id:int}", async (int id, DetallePedido detalle, IDetallePedidosLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                detalle.IdDetallePedido = id;
                var error = await logica.ActualizarAsync(detalle);
                return error is null ? Results.Ok(detalle) : Results.BadRequest(error);
            });

            group.MapDelete("/{id:int}", async (int id, IDetallePedidosLogica logica) =>
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
