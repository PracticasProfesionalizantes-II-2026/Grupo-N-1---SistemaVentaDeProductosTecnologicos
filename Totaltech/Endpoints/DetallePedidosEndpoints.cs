using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class DetallePedidosEndpoints
    {
        public static void MapDetallePedidosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/detallepedidos").WithTags("Detalle pedidos");

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
                await logica.CrearAsync(detalle);
                return Results.Created($"/detallepedidos/{detalle.IdDetallePedido}", detalle);
            });

            group.MapPut("/{id:int}", async (int id, DetallePedido detalle, IDetallePedidosLogica logica) =>
            {
                if (id != detalle.IdDetallePedido)
                {
                    return Results.BadRequest("El id de la URL no coincide con el id del body.");
                }

                var existente = await logica.ObtenerPorIdAsync(id);
                if (existente is null)
                {
                    return Results.NotFound();
                }

                await logica.ActualizarAsync(detalle);
                return Results.NoContent();
            });

            group.MapDelete("/{id:int}", async (int id, IDetallePedidosLogica logica) =>
            {
                var detalle = await logica.ObtenerPorIdAsync(id);
                if (detalle is null)
                {
                    return Results.NotFound();
                }

                await logica.EliminarAsync(detalle);
                return Results.NoContent();
            });
        }
    }
}
