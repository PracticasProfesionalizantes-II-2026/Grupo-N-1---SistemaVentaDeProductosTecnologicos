using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class ProductosEndpoints
    {
        public static void MapProductosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/productos").WithTags("Productos");
            // obtener todos los productos
            group.MapGet("/", async (IProductosLogica logica) =>
            {
                var productos = await logica.ObtenerTodosAsync();
                return Results.Ok(productos);
            });
            // obtener un producto por su id
            group.MapGet("/{id:int}", async (int id, IProductosLogica logica) =>
            {
                var producto = await logica.ObtenerPorIdAsync(id);
                return producto is null ? Results.NotFound() : Results.Ok(producto);
            });
            // crear un nuevo producto
            group.MapPost("/", async (Producto producto, IProductosLogica logica) =>
            {
                var error = await logica.CrearAsync(producto);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/productos/{producto.IdProducto}", producto);
            });
            // actualizar un producto existente
            group.MapPut("/{id:int}", async (int id, Producto producto, IProductosLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                producto.IdProducto = id;
                var error = await logica.ActualizarAsync(producto);
                return error is null ? Results.Ok(producto) : Results.BadRequest(error);
            });
            // eliminar un producto
            group.MapDelete("/{id:int}", async (int id, IProductosLogica logica) =>
            {
                try
                {
                    var eliminado = await logica.EliminarAsync(id);
                    return eliminado ? Results.NoContent() : Results.NotFound();
                }
                catch (DbUpdateException)
                {
                    return Results.Conflict("No se puede eliminar porque hay datos relacionados.");
                }
            });
            // buscar productos por nombre o descripción
            group.MapGet("/buscar", async (string? texto, IProductosLogica logica) =>
            {
                var productos = await logica.BuscarAsync(texto);
                return Results.Ok(productos);
            });
            // obtener productos por categoría
            group.MapGet("/categoria/{idCategoria:int}", async (int idCategoria, IProductosLogica logica) =>
            {
                var productos = await logica.ObtenerPorCategoriaAsync(idCategoria);
                return Results.Ok(productos);
            });
            // obtener productos disponibles
            group.MapGet("/disponibles", async (IProductosLogica logica) =>
            {
                var productos = await logica.ObtenerDisponiblesAsync();
                return Results.Ok(productos);
            });

            // actualizar stock de un producto
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
