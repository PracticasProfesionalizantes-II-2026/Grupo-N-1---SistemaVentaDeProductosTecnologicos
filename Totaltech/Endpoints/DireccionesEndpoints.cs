using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class DireccionesEndpoints
    {
        public static void MapDireccionesEndpoints(this WebApplication app)
        {
            // Estos endpoints traducen HTTP y delegan reglas de negocio a la capa de logica.
            var group = app.MapGroup("/direcciones").WithTags("Direcciones");

            group.MapGet("/", async (IDireccionesLogica logica) =>
            {
                var direcciones = await logica.ObtenerTodosAsync();
                return Results.Ok(direcciones.Select(direccion => direccion.ToResponse()));
            });

            group.MapGet("/{id:int}", async (int id, IDireccionesLogica logica) =>
            {
                var direccion = await logica.ObtenerPorIdAsync(id);
                return direccion is null ? Results.NotFound() : Results.Ok(direccion.ToResponse());
            });

            group.MapPost("/", async (CrearDireccionRequest request, IDireccionesLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearValidadoAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, direccion => Results.Created($"/direcciones/{direccion.IdDireccion}", direccion.ToResponse()));
                });
            });

            group.MapPut("/{id:int}", async (int id, ActualizarDireccionRequest request, IDireccionesLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarValidadoAsync(id, request.ToEntity(id));
                    return EndpointResults.FromResult(resultado, direccion => Results.Ok(direccion.ToResponse()));
                });
            });

            group.MapDelete("/{id:int}", async (int id, IDireccionesLogica logica) =>
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
