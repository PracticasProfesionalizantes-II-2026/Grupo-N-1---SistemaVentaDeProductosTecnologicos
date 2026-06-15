using Totaltech.Entidades;
using Totaltech.Logica;

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

            group.MapPost("/", async (Pedido pedido, IPedidosLogica logica) =>
            {
                await logica.CrearAsync(pedido);
                return Results.Created($"/pedidos/{pedido.IdPedido}", pedido);
            });

            group.MapPut("/{id:int}", async (int id, Pedido pedido, IPedidosLogica logica) =>
            {
                if (id != pedido.IdPedido)
                {
                    return Results.BadRequest("El id de la URL no coincide con el id del body.");
                }

                var existente = await logica.ObtenerPorIdAsync(id);
                if (existente is null)
                {
                    return Results.NotFound();
                }

                await logica.ActualizarAsync(pedido);
                return Results.NoContent();
            });

            group.MapDelete("/{id:int}", async (int id, IPedidosLogica logica) =>
            {
                var pedido = await logica.ObtenerPorIdAsync(id);
                if (pedido is null)
                {
                    return Results.NotFound();
                }

                await logica.EliminarAsync(pedido);
                return Results.NoContent();
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

            group.MapPost("/{idPedido:int}/pagos", async (int idPedido, Pago pago, IPagosLogica logica) =>
            {
                var creado = await logica.CrearParaPedidoAsync(idPedido, pago);

                if (creado is null)
                {
                    return Results.NotFound();
                }

                return Results.Created($"/pagos/{creado.IdPago}", creado);
            });

            group.MapGet("/{idPedido:int}/pagos", async (int idPedido, IPagosLogica logica) =>
            {
                var pagos = await logica.ObtenerPorPedidoAsync(idPedido);
                return Results.Ok(pagos);
            });
        }
    }
}
