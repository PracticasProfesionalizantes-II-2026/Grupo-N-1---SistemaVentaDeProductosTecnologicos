using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class DetalleCarritosEndpoints
    {
        public static void MapDetalleCarritosEndpoints(this WebApplication app)
        {
            // Estos endpoints traducen HTTP y delegan reglas de negocio a la capa de logica.
            var group = app.MapGroup("/detallecarritos").WithTags("DetalleCarritos");

            group.MapGet("/", async (IDetalleCarritosLogica logica) =>
            {
                var detalles = await logica.ObtenerTodosAsync();
                return Results.Ok(detalles.Select(detalle => detalle.ToResponse()));
            });

            group.MapGet("/{id:int}", async (int id, IDetalleCarritosLogica logica) =>
            {
                var detalle = await logica.ObtenerPorIdAsync(id);
                return detalle is null ? Results.NotFound() : Results.Ok(detalle.ToResponse());
            });

            group.MapPost("/", async (CrearDetalleCarritoRequest request, IDetalleCarritosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearValidadoAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, detalle => Results.Created($"/detallecarritos/{detalle.IdDetalleCarrito}", detalle.ToResponse()));
                });
            });

            group.MapPut("/{id:int}", async (int id, ActualizarDetalleCarritoRequest request, IDetalleCarritosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarValidadoAsync(id, request.ToEntity(id));
                    return EndpointResults.FromResult(resultado, detalle => Results.Ok(detalle.ToResponse()));
                });
            });

            group.MapDelete("/{id:int}", async (int id, IDetalleCarritosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.EliminarPorIdAsync(id);
                    return EndpointResults.FromResult(resultado, () => Results.NoContent());
                });
            });

            group.MapGet("/carrito/{idCarrito:int}", async (int idCarrito, IDetalleCarritosLogica logica) =>
            {
                var detalles = await logica.ObtenerPorCarritoAsync(idCarrito);
                return Results.Ok(detalles.Select(detalle => detalle.ToResponse()));
            });
        }
    }
}
