using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class PedidosEndpoints
    {
        public static void MapPedidosEndpoints(this WebApplication app)
        {
            // Estos endpoints traducen HTTP y delegan reglas de negocio a la capa de logica.
            var group = app.MapGroup("/pedidos").WithTags("Pedidos");

            group.MapGet("/", async (IPedidosLogica logica) =>
            {
                var pedidos = await logica.ObtenerTodosAsync();
                return Results.Ok(pedidos.Select(pedido => pedido.ToResponse()));
            });

            group.MapGet("/{id:int}", async (int id, IPedidosLogica logica) =>
            {
                var pedido = await logica.ObtenerPorIdAsync(id);
                return pedido is null ? Results.NotFound() : Results.Ok(pedido.ToResponse());
            });

            group.MapPost("/", async (CrearPedidoRequest request, IPedidosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearValidadoAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, pedido => Results.Created($"/pedidos/{pedido.IdPedido}", pedido.ToResponse()));
                });
            });

            group.MapPut("/{id:int}", async (int id, ActualizarPedidoRequest request, IPedidosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarValidadoAsync(id, request.ToEntity(id));
                    return EndpointResults.FromResult(resultado, pedido => Results.Ok(pedido.ToResponse()));
                });
            });

            group.MapDelete("/{id:int}", async (int id, IPedidosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.EliminarPorIdAsync(id);
                    return EndpointResults.FromResult(resultado, () => Results.NoContent());
                });
            });

            group.MapGet("/usuario/{idUsuario:int}", async (int idUsuario, IPedidosLogica logica) =>
            {
                var pedidos = await logica.ObtenerPorUsuarioAsync(idUsuario);
                return Results.Ok(pedidos.Select(pedido => pedido.ToResponse()));
            });

            group.MapGet("/estado/{estado}", async (EstadoPedido estado, IPedidosLogica logica) =>
            {
                var pedidos = await logica.ObtenerPorEstadoAsync(estado);
                return Results.Ok(pedidos.Select(pedido => pedido.ToResponse()));
            });

            group.MapPatch("/{id:int}/estado", async (int id, ActualizarEstadoPedidoRequest request, IPedidosLogica logica) =>
            {
                var actualizado = await logica.ActualizarEstadoAsync(id, request.Estado);
                return actualizado ? Results.NoContent() : Results.NotFound();
            });

            group.MapPost("/{idPedido:int}/pagos", async (int idPedido, CrearPagoParaPedidoRequest request, IPagosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearParaPedidoAsync(idPedido, request.ToEntity(idPedido));
                    return EndpointResults.FromResult(resultado, pago => Results.Created($"/pagos/{pago.IdPago}", pago.ToResponse()));
                });
            });

            group.MapGet("/{idPedido:int}/pagos", async (int idPedido, IPagosLogica logica) =>
            {
                var pagos = await logica.ObtenerPorPedidoAsync(idPedido);
                return Results.Ok(pagos.Select(pago => pago.ToResponse()));
            });
        }
    }
}
