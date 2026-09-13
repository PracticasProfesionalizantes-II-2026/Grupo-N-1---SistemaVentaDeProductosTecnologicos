using System.Security.Claims;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

namespace Totaltech.Endpoints;

public static class PedidosEndpoints
{
    public static void MapPedidosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pedidos").WithTags("Pedidos");

        group.MapGet("/", async (IPedidosLogica logica, ClaimsPrincipal usuarioActual) =>
        {
            var idUsuario = usuarioActual.ObtenerIdUsuario();
            if (!idUsuario.HasValue)
            {
                return Results.Unauthorized();
            }

            var pedidos = usuarioActual.EsAdministrador()
                ? await logica.ObtenerTodosAsync()
                : await logica.ObtenerPorUsuarioAsync(idUsuario.Value);
            return Results.Ok(pedidos.Select(pedido => pedido.ToResponse()));
        }).RequireAuthorization();

        group.MapGet("/{id:int}", async (int id, IPedidosLogica logica, ClaimsPrincipal usuarioActual) =>
        {
            var pedido = await logica.ObtenerPorIdAsync(id);
            return pedido is null || !usuarioActual.PuedeAcceder(pedido.IdUsuario)
                ? Results.NotFound()
                : Results.Ok(pedido.ToResponse());
        }).RequireAuthorization();

        group.MapGet("/{id:int}/detalles", async (
            int id,
            IPedidosLogica pedidosLogica,
            IDetallePedidosLogica detallesLogica,
            ClaimsPrincipal usuarioActual) =>
        {
            var pedido = await pedidosLogica.ObtenerPorIdAsync(id);
            if (pedido is null || !usuarioActual.PuedeAcceder(pedido.IdUsuario))
            {
                return Results.NotFound();
            }

            var detalles = await detallesLogica.ObtenerPorPedidoAsync(id);
            return Results.Ok(detalles.Select(detalle => detalle.ToResponse()));
        }).RequireAuthorization();

        group.MapGet("/usuario/{idUsuario:int}", async (
            int idUsuario,
            IPedidosLogica logica,
            ClaimsPrincipal usuarioActual) =>
        {
            if (!usuarioActual.PuedeAcceder(idUsuario))
            {
                return Results.NotFound();
            }

            var pedidos = await logica.ObtenerPorUsuarioAsync(idUsuario);
            return Results.Ok(pedidos.Select(pedido => pedido.ToResponse()));
        }).RequireAuthorization();

        group.MapGet("/estado/{estado}", async (EstadoPedido estado, IPedidosLogica logica) =>
        {
            if (!Enum.IsDefined(estado))
            {
                return Results.BadRequest("El estado del pedido no es valido.");
            }

            var pedidos = await logica.ObtenerPorEstadoAsync(estado);
            return Results.Ok(pedidos.Select(pedido => pedido.ToResponse()));
        }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

        group.MapPatch("/{id:int}/estado", async (
            int id,
            ActualizarEstadoPedidoRequest request,
            IPedidosLogica logica) =>
        {
            var resultado = await logica.ActualizarEstadoAsync(id, request.Estado);
            return resultado.Estado switch
            {
                EstadoOperacionDominio.Exitoso => Results.NoContent(),
                EstadoOperacionDominio.NoEncontrado => Results.NotFound(),
                EstadoOperacionDominio.Conflicto => Results.Conflict(resultado.Error),
                _ => Results.BadRequest(resultado.Error)
            };
        }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

        group.MapPost("/{idPedido:int}/pagos", async (
            int idPedido,
            CrearPagoParaPedidoRequest request,
            IPagosLogica logica) =>
        {
            var pago = new Pago
            {
                IdPedido = idPedido,
                FechaPago = request.FechaPago ?? default,
                MetodoPago = request.MetodoPago,
                Monto = request.Monto,
                Estado = EstadoPago.Pendiente
            };

            var error = await logica.CrearParaPedidoAsync(idPedido, pago);
            if (error is not null)
            {
                return Results.BadRequest(error);
            }

            return Results.Created($"/pagos/{pago.IdPago}", pago.ToResponse());
        }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

        group.MapGet("/{idPedido:int}/pagos", async (
            int idPedido,
            IPagosLogica logica,
            IPedidosLogica pedidosLogica,
            ClaimsPrincipal usuarioActual) =>
        {
            var pedido = await pedidosLogica.ObtenerPorIdAsync(idPedido);
            if (pedido is null || !usuarioActual.PuedeAcceder(pedido.IdUsuario))
            {
                return Results.NotFound();
            }

            var pagos = await logica.ObtenerPorPedidoAsync(idPedido);
            return Results.Ok(pagos.Select(pago => pago.ToResponse()));
        }).RequireAuthorization();
    }
}
