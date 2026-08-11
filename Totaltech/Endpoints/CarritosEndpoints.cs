using Microsoft.EntityFrameworkCore;
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

            // obtener todos los carritos
            group.MapGet("/", async (ICarritosLogica logica) =>
            {
                var carritos = await logica.ObtenerTodosAsync();
                return Results.Ok(carritos);
            });

            // obtener un carrito por su id
            group.MapGet("/{id:int}", async (int id, ICarritosLogica logica) =>
            {
                var carrito = await logica.ObtenerPorIdAsync(id);
                return carrito is null ? Results.NotFound() : Results.Ok(carrito);
            });

            // crear un nuevo carrito
            group.MapPost("/", async (CarritoRequest request, ICarritosLogica logica) =>
            {
                var carrito = request.ToEntity();
                var error = await logica.CrearAsync(carrito);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/carritos/{carrito.IdCarrito}", carrito);
            });

            // actualizar un carrito existente
            group.MapPut("/{id:int}", async (int id, CarritoRequest request, ICarritosLogica logica) =>
            {
                var carrito = await logica.ObtenerPorIdAsync(id);
                if (carrito is null)
                {
                    return Results.NotFound();
                }

                carrito.IdUsuario = request.IdUsuario;
                carrito.FechaCreacion = request.FechaCreacion;
                carrito.Estado = request.Estado;
                var error = await logica.ActualizarAsync(carrito);
                return error is null ? Results.Ok(carrito) : Results.BadRequest(error);
            });

            // eliminar un carrito
            group.MapDelete("/{id:int}", async (int id, ICarritosLogica logica) =>
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

            // obtener carritos por usuario
            group.MapGet("/usuario/{idUsuario:int}", async (int idUsuario, ICarritosLogica logica) =>
            {
                var carritos = await logica.ObtenerPorUsuarioAsync(idUsuario);
                return Results.Ok(carritos);
            });

            // agregar un producto a un carrito
            group.MapPost("/{idCarrito:int}/productos", async (int idCarrito, AgregarProductoCarritoDto request, ICarritosLogica logica) =>
            {
                var resultado = await logica.AgregarProductoAsync(idCarrito, request);
                if (resultado.Error is not null)
                {
                    return EsNoEncontrado(resultado.Error) ? Results.NotFound(resultado.Error) : Results.BadRequest(resultado.Error);
                }

                return Results.Created($"/detallecarritos/{resultado.Detalle!.IdDetalleCarrito}", resultado.Detalle);
            });

            // eliminar un producto de un carrito
            group.MapDelete("/{idCarrito:int}/productos/{idProducto:int}", async (int idCarrito, int idProducto, ICarritosLogica logica) =>
            {
                var error = await logica.EliminarProductoAsync(idCarrito, idProducto);
                if (error is not null)
                {
                    return EsNoEncontrado(error) ? Results.NotFound(error) : Results.BadRequest(error);
                }

                return Results.NoContent();
            });

            // confirmar un carrito y crear un pedido
            group.MapPost("/{idCarrito:int}/confirmar", async (int idCarrito, ConfirmarCarritoDto request, ICarritosLogica logica) =>
            {
                var resultado = await logica.ConfirmarAsync(idCarrito, request);
                if (resultado.Error is not null)
                {
                    return EsNoEncontrado(resultado.Error) ? Results.NotFound(resultado.Error) : Results.BadRequest(resultado.Error);
                }

                return Results.Created($"/pedidos/{resultado.Pedido!.IdPedido}", resultado.Pedido);
            });
        }

        /// <summary>
        /// Verifica si el mensaje de error indica que el recurso no fue encontrado. CONSULTAR A GABI O EMI SI ESTO ES NECESARIO
        /// </summary>
        private static bool EsNoEncontrado(string error)
        {
            return error.Contains("no existe", StringComparison.OrdinalIgnoreCase);
        }
    }
}
