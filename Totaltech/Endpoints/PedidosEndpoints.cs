using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class PedidosEndpoints
    {
        public static void MapPedidosEndpoints(this WebApplication app)
        {
            var group = app.MapCrud<Pedido, IPedidosLogica>("/pedidos", "Pedidos", pedido => pedido.IdPedido);

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
        }
    }
}
