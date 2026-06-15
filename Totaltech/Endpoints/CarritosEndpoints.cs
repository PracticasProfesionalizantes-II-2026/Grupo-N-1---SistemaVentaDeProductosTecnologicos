using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class CarritosEndpoints
    {
        public static void MapCarritosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/carritos").WithTags("Carritos");

            group.MapGet("/", async (ICarritosLogica logica) =>
            {
                var carritos = await logica.ObtenerTodosAsync();
                return Results.Ok(carritos);
            });

            group.MapGet("/{id:int}", async (int id, ICarritosLogica logica) =>
            {
                var carrito = await logica.ObtenerPorIdAsync(id);
                return carrito is null ? Results.NotFound() : Results.Ok(carrito);
            });

            group.MapPost("/", async (Carrito carrito, ICarritosLogica logica) =>
            {
                await logica.CrearAsync(carrito);
                return Results.Created($"/carritos/{carrito.IdCarrito}", carrito);
            });

            group.MapPut("/{id:int}", async (int id, Carrito carrito, ICarritosLogica logica) =>
            {
                if (id != carrito.IdCarrito)
                {
                    return Results.BadRequest("El id de la URL no coincide con el id del body.");
                }

                var existente = await logica.ObtenerPorIdAsync(id);
                if (existente is null)
                {
                    return Results.NotFound();
                }

                await logica.ActualizarAsync(carrito);
                return Results.NoContent();
            });

            group.MapDelete("/{id:int}", async (int id, ICarritosLogica logica) =>
            {
                var carrito = await logica.ObtenerPorIdAsync(id);
                if (carrito is null)
                {
                    return Results.NotFound();
                }

                await logica.EliminarAsync(carrito);
                return Results.NoContent();
            });

            group.MapGet("/usuario/{idUsuario:int}", async (int idUsuario, ICarritosLogica logica) =>
            {
                var carritos = await logica.ObtenerPorUsuarioAsync(idUsuario);
                return Results.Ok(carritos);
            });

            group.MapPost("/{idCarrito:int}/productos", async (int idCarrito, AgregarProductoCarritoDto request, ICarritosLogica logica) =>
            {
                if (request.Cantidad <= 0)
                {
                    return Results.BadRequest("La cantidad debe ser mayor a cero.");
                }

                var detalle = await logica.AgregarProductoAsync(idCarrito, request);

                if (detalle is null)
                {
                    return Results.NotFound();
                }

                return Results.Created($"/detallecarritos/{detalle.IdDetalleCarrito}", detalle);
            });

            group.MapDelete("/{idCarrito:int}/productos/{idProducto:int}", async (int idCarrito, int idProducto, ICarritosLogica logica) =>
            {
                var eliminado = await logica.EliminarProductoAsync(idCarrito, idProducto);
                return eliminado ? Results.NoContent() : Results.NotFound();
            });
        }
    }
}
