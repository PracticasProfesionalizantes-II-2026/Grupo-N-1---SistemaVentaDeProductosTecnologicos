using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class PagosEndpoints
    {
        public static void MapPagosEndpoints(this WebApplication app)
        {
            // Estos endpoints traducen HTTP y delegan reglas de negocio a la capa de logica.
            var group = app.MapGroup("/pagos").WithTags("Pagos");

            group.MapGet("/", async (IPagosLogica logica) =>
            {
                var pagos = await logica.ObtenerTodosAsync();
                return Results.Ok(pagos.Select(pago => pago.ToResponse()));
            });

            group.MapGet("/{id:int}", async (int id, IPagosLogica logica) =>
            {
                var pago = await logica.ObtenerPorIdAsync(id);
                return pago is null ? Results.NotFound() : Results.Ok(pago.ToResponse());
            });

            group.MapPost("/", async (CrearPagoRequest request, IPagosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearValidadoAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, pago => Results.Created($"/pagos/{pago.IdPago}", pago.ToResponse()));
                });
            });

            group.MapPut("/{id:int}", async (int id, ActualizarPagoRequest request, IPagosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarValidadoAsync(id, request.ToEntity(id));
                    return EndpointResults.FromResult(resultado, pago => Results.Ok(pago.ToResponse()));
                });
            });

            group.MapDelete("/{id:int}", async (int id, IPagosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.EliminarPorIdAsync(id);
                    return EndpointResults.FromResult(resultado, () => Results.NoContent());
                });
            });

            group.MapPatch("/{id:int}/estado", async (int id, ActualizarEstadoPagoRequest request, IPagosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarEstadoAsync(id, request.Estado);
                    return EndpointResults.FromResult(resultado, pago => Results.Ok(pago.ToResponse()));
                });
            });
        }
    }
}
