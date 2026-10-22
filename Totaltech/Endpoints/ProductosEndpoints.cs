using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

namespace Totaltech.Endpoints
{
    public static class ProductosEndpoints
    {
        private const string MensajeConflictoStock = "El stock cambió. Recargá el producto antes de guardar los cambios.";

        public static void MapProductosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/productos").WithTags("Productos");
            group.MapGet("/catalogo", async (string? texto, int? idCategoria, decimal? precioMin,
                decimal? precioMax, bool? soloDisponibles, int? pagina, int? tamanoPagina,
                IProductosLogica logica, CancellationToken cancellationToken) =>
            {
                var filtro = new FiltroCatalogoProductos
                {
                    Texto = texto, IdCategoria = idCategoria, PrecioMin = precioMin, PrecioMax = precioMax,
                    SoloDisponibles = soloDisponibles ?? false, Pagina = pagina ?? 1,
                    TamanoPagina = tamanoPagina ?? 12
                };
                var (catalogo, error) = await logica.ObtenerCatalogoAsync(filtro, cancellationToken);
                return error is null
                    ? Results.Ok(catalogo)
                    : Results.BadRequest(new ErrorCatalogoResponse { Mensaje = error });
            }).AllowAnonymous();
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
                if (request.StockOriginal is null or < 0)
                {
                    return Results.BadRequest("El stock original es obligatorio y no puede ser negativo.");
                }

                var producto = await logica.ObtenerPorIdAsync(id);
                if (producto is null)
                {
                    return Results.NotFound();
                }

                if (producto.Stock != request.StockOriginal.Value)
                {
                    return Results.Conflict(MensajeConflictoStock);
                }

                AplicarCambios(producto, request);
                try
                {
                    var error = await logica.ActualizarAsync(producto);
                    return error is null ? Results.Ok(producto) : Results.BadRequest(error);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Results.Conflict(MensajeConflictoStock);
                }
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
                if (request.Stock < 0 || request.StockOriginal is null or < 0)
                {
                    return Results.BadRequest("El stock y el stock original deben estar presentes y no pueden ser negativos.");
                }

                var resultado = await logica.ActualizarStockAsync(id, request.Stock, request.StockOriginal.Value);
                return resultado switch
                {
                    ResultadoActualizacionStock.Actualizado => Results.NoContent(),
                    ResultadoActualizacionStock.NoEncontrado => Results.NotFound(),
                    _ => Results.Conflict(MensajeConflictoStock)
                };
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);
        }

        private static void AplicarCambios(Producto producto, ProductoRequest request)
        {
            producto.Nombre = request.Nombre;
            producto.Descripcion = request.Descripcion;
            if (request.ImagenUrl is not null)
            {
                producto.ImagenUrl = request.ImagenUrl;
            }
            producto.Precio = request.Precio;
            producto.Stock = request.Stock;
            producto.IdCategoria = request.IdCategoria;
            producto.IdProveedor = request.IdProveedor;
        }
    }
}
