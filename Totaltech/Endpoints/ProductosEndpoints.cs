using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class ProductosEndpoints
    {
        public static void MapProductosEndpoints(this WebApplication app)
        {
            // Estos endpoints traducen HTTP y delegan reglas de negocio a la capa de logica.
            var group = app.MapGroup("/productos").WithTags("Productos");

            group.MapGet("/", async (IProductosLogica logica) =>
            {
                var productos = await logica.ObtenerTodosAsync();
                return Results.Ok(productos.Select(producto => producto.ToResponse()));
            });

            group.MapGet("/{id:int}", async (int id, IProductosLogica logica) =>
            {
                var producto = await logica.ObtenerPorIdAsync(id);
                return producto is null ? Results.NotFound() : Results.Ok(producto.ToResponse());
            });

            group.MapPost("/", async (CrearProductoRequest request, IProductosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearValidadoAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, producto => Results.Created($"/productos/{producto.IdProducto}", producto.ToResponse()));
                });
            });

            group.MapPut("/{id:int}", async (int id, ActualizarProductoRequest request, IProductosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarValidadoAsync(id, request.ToEntity(id));
                    return EndpointResults.FromResult(resultado, producto => Results.Ok(producto.ToResponse()));
                });
            });

            group.MapDelete("/{id:int}", async (int id, IProductosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.EliminarPorIdAsync(id);
                    return EndpointResults.FromResult(resultado, () => Results.NoContent());
                });
            });

            group.MapGet("/buscar", async (string? texto, IProductosLogica logica) =>
            {
                var productos = await logica.BuscarAsync(texto);
                return Results.Ok(productos.Select(producto => producto.ToResponse()));
            });

            group.MapGet("/categoria/{idCategoria:int}", async (int idCategoria, IProductosLogica logica) =>
            {
                var productos = await logica.ObtenerPorCategoriaAsync(idCategoria);
                return Results.Ok(productos.Select(producto => producto.ToResponse()));
            });

            group.MapGet("/disponibles", async (IProductosLogica logica) =>
            {
                var productos = await logica.ObtenerDisponiblesAsync();
                return Results.Ok(productos.Select(producto => producto.ToResponse()));
            });

            group.MapPatch("/{id:int}/stock", async (int id, ActualizarStockRequest request, IProductosLogica logica) =>
            {
                if (request.Stock < 0)
                {
                    return Results.BadRequest("El stock no puede ser negativo.");
                }

                var actualizado = await logica.ActualizarStockAsync(id, request.Stock);
                return actualizado ? Results.NoContent() : Results.NotFound();
            });
        }
    }
}
