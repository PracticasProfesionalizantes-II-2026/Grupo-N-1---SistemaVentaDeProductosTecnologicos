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

            group.MapGet("/", async (IPedidosLogica logica) =>
            {
                var pedidos = await logica.ObtenerTodosAsync();
                return Results.Ok(pedidos);
            });

            group.MapGet("/{id:int}", async (int id, IPedidosLogica logica) =>
            {
                var pedido = await logica.ObtenerPorIdAsync(id);
                return pedido is null ? Results.NotFound() : Results.Ok(pedido);
            });

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

            group.MapPut("/{id:int}", async (int id, PedidoRequest request, IPedidosLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                var pedido = request.ToEntity();
                pedido.IdPedido = id;
                var error = await logica.ActualizarAsync(pedido);
                return error is null ? Results.Ok(pedido) : Results.BadRequest(error);
            });

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

            group.MapGet("/usuario/{idUsuario:int}", async (int idUsuario, IPedidosLogica logica) =>
            {
                var pedidos = await logica.ObtenerPorUsuarioAsync(idUsuario);
                return Results.Ok(pedidos);
            });

            group.MapGet("/estado/{estado}", async (EstadoPedido estado, IPedidosLogica logica) =>
            {
                var pedidos = await logica.ObtenerPorEstadoAsync(estado);
                return Results.Ok(pedidos);
            });

            group.MapPatch("/{id:int}/estado", async (int id, ActualizarEstadoPedidoRequest request, IPedidosLogica logica) =>
            {
                var actualizado = await logica.ActualizarEstadoAsync(id, request.Estado);
                return actualizado ? Results.NoContent() : Results.NotFound();
            });

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

            group.MapGet("/{idPedido:int}/pagos", async (int idPedido, IPagosLogica logica) =>
            {
                var pagos = await logica.ObtenerPorPedidoAsync(idPedido);
                return Results.Ok(pagos);
            });
        }
    }
}
