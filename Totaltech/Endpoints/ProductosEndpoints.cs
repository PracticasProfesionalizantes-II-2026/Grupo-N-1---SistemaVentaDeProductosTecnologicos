using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class ProductosEndpoints
    {
        public static void MapProductosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/productos").WithTags("Productos");

            group.MapGet("/", async (IProductosLogica logica) =>
            {
                var productos = await logica.ObtenerTodosAsync();
                return Results.Ok(productos);
            });

            group.MapGet("/{id:int}", async (int id, IProductosLogica logica) =>
            {
                var producto = await logica.ObtenerPorIdAsync(id);
                return producto is null ? Results.NotFound() : Results.Ok(producto);
            });

            group.MapPost("/", async (Producto producto, IProductosLogica logica) =>
            {
                await logica.CrearAsync(producto);
                return Results.Created($"/productos/{producto.IdProducto}", producto);
            });

            group.MapPut("/{id:int}", async (int id, Producto producto, IProductosLogica logica) =>
            {
                if (id != producto.IdProducto)
                {
                    return Results.BadRequest("El id de la URL no coincide con el id del body.");
                }

                var existente = await logica.ObtenerPorIdAsync(id);
                if (existente is null)
                {
                    return Results.NotFound();
                }

                await logica.ActualizarAsync(producto);
                return Results.NoContent();
            });

            group.MapDelete("/{id:int}", async (int id, IProductosLogica logica) =>
            {
                var producto = await logica.ObtenerPorIdAsync(id);
                if (producto is null)
                {
                    return Results.NotFound();
                }

                await logica.EliminarAsync(producto);
                return Results.NoContent();
            });

            group.MapGet("/buscar", async (string? texto, IProductosLogica logica) =>
            {
                var productos = await logica.BuscarAsync(texto);
                return Results.Ok(productos);
            });

            group.MapGet("/categoria/{idCategoria:int}", async (int idCategoria, IProductosLogica logica) =>
            {
                var productos = await logica.ObtenerPorCategoriaAsync(idCategoria);
                return Results.Ok(productos);
            });

            group.MapGet("/disponibles", async (IProductosLogica logica) =>
            {
                var productos = await logica.ObtenerDisponiblesAsync();
                return Results.Ok(productos);
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
