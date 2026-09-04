using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

namespace Totaltech.Endpoints
{
    public static class PedidosEndpoints
    {
        public static void MapPedidosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/pedidos").WithTags("Pedidos");

            // obtener todos los pedidos
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
                return Results.Ok(pedidos);
            }).RequireAuthorization();

            // obtener un pedido por su id
            group.MapGet("/{id:int}", async (int id, IPedidosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                var pedido = await logica.ObtenerPorIdAsync(id);
                return pedido is null || !usuarioActual.PuedeAcceder(pedido.IdUsuario)
                    ? Results.NotFound()
                    : Results.Ok(pedido);
            }).RequireAuthorization();

            // crear un nuevo pedido
            group.MapPost("/", async (PedidoRequest request, IPedidosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                if (!usuarioActual.EsAdministrador())
                {
                    request.IdUsuario = usuarioActual.ObtenerIdUsuario();
                    request.Estado = EstadoPedido.Pendiente;
                }

                var pedido = request.ToEntity();
                var error = await logica.CrearAsync(pedido);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/pedidos/{pedido.IdPedido}", pedido);
            }).RequireAuthorization();

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
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

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
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

            // obtener pedidos por usuario
            group.MapGet("/usuario/{idUsuario:int}", async (int idUsuario, IPedidosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                if (!usuarioActual.PuedeAcceder(idUsuario))
                {
                    return Results.NotFound();
                }

                var pedidos = await logica.ObtenerPorUsuarioAsync(idUsuario);
                return Results.Ok(pedidos);
            }).RequireAuthorization();

            // obtener pedidos por estado
            group.MapGet("/estado/{estado}", async (EstadoPedido estado, IPedidosLogica logica) =>
            {
                if (!Enum.IsDefined(estado))
                {
                    return Results.BadRequest("El estado del pedido no es valido.");
                }

                var pedidos = await logica.ObtenerPorEstadoAsync(estado);
                return Results.Ok(pedidos);
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

            //actualizar el estado de un pedido
            group.MapPatch("/{id:int}/estado", async (int id, ActualizarEstadoPedidoRequest request, IPedidosLogica logica) =>
            {
                if (!Enum.IsDefined(request.Estado))
                {
                    return Results.BadRequest("El estado del pedido no es valido.");
                }

                var actualizado = await logica.ActualizarEstadoAsync(id, request.Estado);
                return actualizado ? Results.NoContent() : Results.NotFound();
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

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
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

            // obtener pagos de un pedido
            group.MapGet("/{idPedido:int}/pagos", async (int idPedido, IPagosLogica logica, IPedidosLogica pedidosLogica, ClaimsPrincipal usuarioActual) =>
            {
                var pedido = await pedidosLogica.ObtenerPorIdAsync(idPedido);
                if (pedido is null || !usuarioActual.PuedeAcceder(pedido.IdUsuario))
                {
                    return Results.NotFound();
                }

                var pagos = await logica.ObtenerPorPedidoAsync(idPedido);
                return Results.Ok(pagos);
            }).RequireAuthorization();
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
