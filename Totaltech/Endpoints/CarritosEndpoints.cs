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
                var error = await logica.CrearAsync(carrito);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/carritos/{carrito.IdCarrito}", carrito);
            });

            group.MapPut("/{id:int}", async (int id, Carrito carrito, ICarritosLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                carrito.IdCarrito = id;
                var error = await logica.ActualizarAsync(carrito);
                return error is null ? Results.Ok(carrito) : Results.BadRequest(error);
            });

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

            group.MapGet("/usuario/{idUsuario:int}", async (int idUsuario, ICarritosLogica logica) =>
            {
                var carritos = await logica.ObtenerPorUsuarioAsync(idUsuario);
                return Results.Ok(carritos);
            });

            group.MapPost("/{idCarrito:int}/productos", async (int idCarrito, AgregarProductoCarritoDto request, ICarritosLogica logica) =>
            {
                var resultado = await logica.AgregarProductoAsync(idCarrito, request);
                if (resultado.Error is not null)
                {
                    return EsNoEncontrado(resultado.Error) ? Results.NotFound(resultado.Error) : Results.BadRequest(resultado.Error);
                }

                return Results.Created($"/detallecarritos/{resultado.Detalle!.IdDetalleCarrito}", resultado.Detalle);
            });

            group.MapDelete("/{idCarrito:int}/productos/{idProducto:int}", async (int idCarrito, int idProducto, ICarritosLogica logica) =>
            {
                var error = await logica.EliminarProductoAsync(idCarrito, idProducto);
                if (error is not null)
                {
                    return EsNoEncontrado(error) ? Results.NotFound(error) : Results.BadRequest(error);
                }

                return Results.NoContent();
            });

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

        private static bool EsNoEncontrado(string error)
        {
            return error.Contains("no existe", StringComparison.OrdinalIgnoreCase);
        }
    }
}
