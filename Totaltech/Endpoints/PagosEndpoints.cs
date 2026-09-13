using System.Security.Claims;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

namespace Totaltech.Endpoints;

public static class PagosEndpoints
{
    public static void MapPagosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pagos").WithTags("Pagos");

        group.MapGet("/", async (IPagosLogica logica) =>
        {
            var pagos = await logica.ObtenerTodosAsync();
            return Results.Ok(pagos.Select(pago => pago.ToResponse()));
        }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

        group.MapGet("/{id:int}", async (
            int id,
            IPagosLogica logica,
            IPedidosLogica pedidosLogica,
            ClaimsPrincipal usuarioActual) =>
        {
            var pago = await logica.ObtenerPorIdAsync(id);
            if (pago is null)
            {
                return Results.NotFound();
            }

            var pedido = await pedidosLogica.ObtenerPorIdAsync(pago.IdPedido);
            return pedido is null || !usuarioActual.PuedeAcceder(pedido.IdUsuario)
                ? Results.NotFound()
                : Results.Ok(pago.ToResponse());
        }).RequireAuthorization();

        group.MapPatch("/{id:int}/estado", async (
            int id,
            ActualizarEstadoPagoRequest request,
            IPagosLogica logica) =>
        {
            var resultado = await logica.ActualizarEstadoAsync(id, request.Estado);
            if (resultado.Estado == EstadoOperacionDominio.NoEncontrado)
            {
                return Results.NotFound();
            }

            if (resultado.Estado == EstadoOperacionDominio.Conflicto)
            {
                return Results.Conflict(resultado.Error);
            }

            if (resultado.Estado == EstadoOperacionDominio.Invalido)
            {
                return Results.BadRequest(resultado.Error);
            }

            var pago = await logica.ObtenerPorIdAsync(id);
            return Results.Ok(pago!.ToResponse());
        }).RequireAuthorization(Autorizacion.PoliticaAdministrador);
    }
}
