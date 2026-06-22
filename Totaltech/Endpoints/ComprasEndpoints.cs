using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class ComprasEndpoints
    {
        public static void MapComprasEndpoints(this WebApplication app)
        {
            // Estos endpoints traducen HTTP y delegan reglas de negocio a la capa de logica.
            var group = app.MapGroup("/compras").WithTags("Compras");

            group.MapGet("/", async (IComprasLogica logica) =>
            {
                var compras = await logica.ObtenerTodosAsync();
                return Results.Ok(compras.Select(compra => compra.ToResponse()));
            });

            group.MapGet("/{id:int}", async (int id, IComprasLogica logica) =>
            {
                var compra = await logica.ObtenerPorIdAsync(id);
                return compra is null ? Results.NotFound() : Results.Ok(compra.ToResponse());
            });

            group.MapPost("/", async (CrearCompraRequest request, IComprasLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearValidadoAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, compra => Results.Created($"/compras/{compra.IdCompra}", compra.ToResponse()));
                });
            });

            group.MapPut("/{id:int}", async (int id, ActualizarCompraRequest request, IComprasLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarValidadoAsync(id, request.ToEntity(id));
                    return EndpointResults.FromResult(resultado, compra => Results.Ok(compra.ToResponse()));
                });
            });

            group.MapDelete("/{id:int}", async (int id, IComprasLogica logica) =>
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
