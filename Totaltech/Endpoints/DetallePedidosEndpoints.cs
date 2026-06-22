using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class DetallePedidosEndpoints
    {
        public static void MapDetallePedidosEndpoints(this WebApplication app)
        {
            // Estos endpoints traducen HTTP y delegan reglas de negocio a la capa de logica.
            var group = app.MapGroup("/detallepedidos").WithTags("DetallePedidos");

            group.MapGet("/", async (IDetallePedidosLogica logica) =>
            {
                var detalles = await logica.ObtenerTodosAsync();
                return Results.Ok(detalles.Select(detalle => detalle.ToResponse()));
            });

            group.MapGet("/{id:int}", async (int id, IDetallePedidosLogica logica) =>
            {
                var detalle = await logica.ObtenerPorIdAsync(id);
                return detalle is null ? Results.NotFound() : Results.Ok(detalle.ToResponse());
            });

            group.MapPost("/", async (CrearDetallePedidoRequest request, IDetallePedidosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearValidadoAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, detalle => Results.Created($"/detallepedidos/{detalle.IdDetallePedido}", detalle.ToResponse()));
                });
            });

            group.MapPut("/{id:int}", async (int id, ActualizarDetallePedidoRequest request, IDetallePedidosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarValidadoAsync(id, request.ToEntity(id));
                    return EndpointResults.FromResult(resultado, detalle => Results.Ok(detalle.ToResponse()));
                });
            });

            group.MapDelete("/{id:int}", async (int id, IDetallePedidosLogica logica) =>
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
