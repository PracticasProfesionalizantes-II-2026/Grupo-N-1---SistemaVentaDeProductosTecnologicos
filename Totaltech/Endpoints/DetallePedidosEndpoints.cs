using System.Security.Claims;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

namespace Totaltech.Endpoints;

public static class DetallePedidosEndpoints
{
    public static void MapDetallePedidosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/detallepedidos").WithTags("DetallePedidos");

        group.MapGet("/", async (IDetallePedidosLogica logica) =>
        {
            var detalles = await logica.ObtenerTodosAsync();
            return Results.Ok(detalles.Select(detalle => detalle.ToResponse()));
        }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

        group.MapGet("/{id:int}", async (
            int id,
            IDetallePedidosLogica logica,
            IPedidosLogica pedidosLogica,
            ClaimsPrincipal usuarioActual) =>
        {
            var detalle = await logica.ObtenerPorIdAsync(id);
            if (detalle is null)
            {
                return Results.NotFound();
            }

            var pedido = await pedidosLogica.ObtenerPorIdAsync(detalle.IdPedido);
            return pedido is null || !usuarioActual.PuedeAcceder(pedido.IdUsuario)
                ? Results.NotFound()
                : Results.Ok(detalle.ToResponse());
        }).RequireAuthorization();
    }
}
