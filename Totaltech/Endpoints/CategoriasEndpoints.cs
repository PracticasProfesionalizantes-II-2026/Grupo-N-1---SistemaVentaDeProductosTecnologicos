using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class CategoriasEndpoints
    {
        public static void MapCategoriasEndpoints(this WebApplication app)
        {
            // Estos endpoints traducen HTTP y delegan reglas de negocio a la capa de logica.
            var group = app.MapGroup("/categorias").WithTags("Categorias");

            group.MapGet("/", async (ICategoriasLogica logica) =>
            {
                var categorias = await logica.ObtenerTodosAsync();
                return Results.Ok(categorias.Select(categoria => categoria.ToResponse()));
            });

            group.MapGet("/{id:int}", async (int id, ICategoriasLogica logica) =>
            {
                var categoria = await logica.ObtenerPorIdAsync(id);
                return categoria is null ? Results.NotFound() : Results.Ok(categoria.ToResponse());
            });

            group.MapPost("/", async (CrearCategoriaRequest request, ICategoriasLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearValidadoAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, categoria => Results.Created($"/categorias/{categoria.IdCategoria}", categoria.ToResponse()));
                });
            });

            group.MapPut("/{id:int}", async (int id, ActualizarCategoriaRequest request, ICategoriasLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarValidadoAsync(id, request.ToEntity(id));
                    return EndpointResults.FromResult(resultado, categoria => Results.Ok(categoria.ToResponse()));
                });
            });

            group.MapDelete("/{id:int}", async (int id, ICategoriasLogica logica) =>
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
