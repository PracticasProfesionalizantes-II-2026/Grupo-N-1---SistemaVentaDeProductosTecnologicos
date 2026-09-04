using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

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
            }).AllowAnonymous();
            // obtener un producto por su id
            group.MapGet("/{id:int}", async (int id, IProductosLogica logica) =>
            {
                var producto = await logica.ObtenerPorIdAsync(id);
                return producto is null ? Results.NotFound() : Results.Ok(producto);
            }).AllowAnonymous();
            // crear un nuevo producto
            group.MapPost("/", async (ProductoRequest request, IProductosLogica logica) =>
            {
                var producto = request.ToEntity();
                var error = await logica.CrearAsync(producto);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/productos/{producto.IdProducto}", producto);
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);
            // actualizar un producto existente
            group.MapPut("/{id:int}", async (int id, ProductoRequest request, IProductosLogica logica) =>
            {
                var producto = await logica.ObtenerPorIdAsync(id);
                if (producto is null)
                {
                    return Results.NotFound();
                }

                AplicarCambios(producto, request);
                var error = await logica.ActualizarAsync(producto);
                return error is null ? Results.Ok(producto) : Results.BadRequest(error);
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);
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
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);
            // buscar productos por nombre o descripción
            group.MapGet("/buscar", async (string? texto, IProductosLogica logica) =>
            {
                var productos = await logica.BuscarAsync(texto);
                return Results.Ok(productos);
            }).AllowAnonymous();
            // obtener productos por categoría
            group.MapGet("/categoria/{idCategoria:int}", async (int idCategoria, IProductosLogica logica) =>
            {
                var productos = await logica.ObtenerPorCategoriaAsync(idCategoria);
                return Results.Ok(productos);
            }).AllowAnonymous();
            // obtener productos disponibles
            group.MapGet("/disponibles", async (IProductosLogica logica) =>
            {
                var productos = await logica.ObtenerDisponiblesAsync();
                return Results.Ok(productos);
            }).AllowAnonymous();

            // actualizar stock de un producto
            group.MapPatch("/{id:int}/stock", async (int id, ActualizarStockRequest request, IProductosLogica logica) =>
            {
                if (request.Stock < 0)
                {
                    return Results.BadRequest("El stock no puede ser negativo.");
                }

                var actualizado = await logica.ActualizarStockAsync(id, request.Stock);
                return actualizado ? Results.NoContent() : Results.NotFound();
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);
        }

        private static void AplicarCambios(Producto producto, ProductoRequest request)
        {
            producto.Nombre = request.Nombre;
            producto.Descripcion = request.Descripcion;
            producto.Precio = request.Precio;
            producto.Stock = request.Stock;
            producto.IdCategoria = request.IdCategoria;
            producto.IdProveedor = request.IdProveedor;
        }
    }
}
