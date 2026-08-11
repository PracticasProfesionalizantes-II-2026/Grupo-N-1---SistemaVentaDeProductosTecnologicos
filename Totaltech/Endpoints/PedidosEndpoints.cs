using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class PedidosEndpoints
    {
        public static void MapPedidosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/pedidos").WithTags("Pedidos");

            // obtener todos los pedidos
            group.MapGet("/", async (IPedidosLogica logica) =>
            {
                var pedidos = await logica.ObtenerTodosAsync();
                return Results.Ok(pedidos);
            });

            // obtener un pedido por su id
            group.MapGet("/{id:int}", async (int id, IPedidosLogica logica) =>
            {
                var pedido = await logica.ObtenerPorIdAsync(id);
                return pedido is null ? Results.NotFound() : Results.Ok(pedido);
            });

            // crear un nuevo pedido
            group.MapPost("/", async (PedidoRequest request, IPedidosLogica logica) =>
            {
                var pedido = request.ToEntity();
                var error = await logica.CrearAsync(pedido);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/pedidos/{pedido.IdPedido}", pedido);
            });

            // actualizar un pedido existente
            group.MapPut("/{id:int}", async (int id, PedidoRequest request, IPedidosLogica logica) =>
            {
                var pedido = await logica.ObtenerPorIdAsync(id);
                if (pedido is null)
                {
                    return Results.NotFound();
                }

                AplicarCambios(pedido, request);
                var error = await logica.ActualizarAsync(pedido);
                return error is null ? Results.Ok(pedido) : Results.BadRequest(error);
            });

            // eliminar un pedido
            group.MapDelete("/{id:int}", async (int id, IPedidosLogica logica) =>
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

            // obtener pedidos por usuario
            group.MapGet("/usuario/{idUsuario:int}", async (int idUsuario, IPedidosLogica logica) =>
            {
                var pedidos = await logica.ObtenerPorUsuarioAsync(idUsuario);
                return Results.Ok(pedidos);
            });

            // obtener pedidos por estado
            group.MapGet("/estado/{estado}", async (EstadoPedido estado, IPedidosLogica logica) =>
            {
                var pedidos = await logica.ObtenerPorEstadoAsync(estado);
                return Results.Ok(pedidos);
            });

            //actualizar el estado de un pedido
            group.MapPatch("/{id:int}/estado", async (int id, ActualizarEstadoPedidoRequest request, IPedidosLogica logica) =>
            {
                var actualizado = await logica.ActualizarEstadoAsync(id, request.Estado);
                return actualizado ? Results.NoContent() : Results.NotFound();
            });

            // agregar un pago a un pedido
            group.MapPost("/{idPedido:int}/pagos", async (int idPedido, CrearPagoParaPedidoRequest request, IPagosLogica logica) =>
            {
                var pago = new Pago
                {
                    IdPedido = idPedido,
                    FechaPago = request.FechaPago ?? default,
                    MetodoPago = request.MetodoPago,
                    Monto = request.Monto,
                    Estado = request.Estado
                };

                var error = await logica.CrearParaPedidoAsync(idPedido, pago);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/pagos/{pago.IdPago}", pago);
            });

            // obtener pagos de un pedido
            group.MapGet("/{idPedido:int}/pagos", async (int idPedido, IPagosLogica logica) =>
            {
                var pagos = await logica.ObtenerPorPedidoAsync(idPedido);
                return Results.Ok(pagos);
            });
        }

        private static void AplicarCambios(Pedido pedido, PedidoRequest request)
        {
            pedido.IdUsuario = request.IdUsuario;
            pedido.FechaPedido = request.FechaPedido;
            pedido.Estado = request.Estado;
            pedido.IdDireccion = request.IdDireccion;
        }
    }
}
