using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class ProductosEndpoints
    {
        public static void MapProductosEndpoints(this WebApplication app)
        {
            var group = app.MapCrud<Producto, IProductosLogica>("/productos", "Productos", producto => producto.IdProducto);

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
