using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

namespace Totaltech.Endpoints
{
    public static class DetallePedidosEndpoints
    {
        public static void MapDetallePedidosEndpoints(this WebApplication app)
        {

            var group = app.MapGroup("/detallepedidos").WithTags("DetallePedidos");

            // obtener todos los detalles de pedidos
            group.MapGet("/", async (IDetallePedidosLogica logica) =>
            {
                var detalles = await logica.ObtenerTodosAsync();
                return Results.Ok(detalles);
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

            // obtener un detalle de pedido por su id
            group.MapGet("/{id:int}", async (int id, IDetallePedidosLogica logica, IPedidosLogica pedidosLogica, ClaimsPrincipal usuarioActual) =>
            {
                var detalle = await logica.ObtenerPorIdAsync(id);
                return detalle is null || !await EsPedidoAccesibleAsync(detalle.IdPedido, pedidosLogica, usuarioActual)
                    ? Results.NotFound()
                    : Results.Ok(detalle);
            }).RequireAuthorization();

            // crear un nuevo detalle de pedido
            group.MapPost("/", async (DetallePedidoRequest request, IDetallePedidosLogica logica) =>
            {
                var detalle = request.ToEntity();
                var error = await logica.CrearAsync(detalle);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/detallepedidos/{detalle.IdDetallePedido}", detalle);
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

            // actualizar un detalle de pedido existente
            group.MapPut("/{id:int}", async (int id, DetallePedidoRequest request, IDetallePedidosLogica logica) =>
            {
                var detalle = await logica.ObtenerPorIdAsync(id);
                if (detalle is null)
                {
                    return Results.NotFound();
                }

                detalle.IdPedido = request.IdPedido;
                detalle.IdProducto = request.IdProducto;
                detalle.Cantidad = request.Cantidad;
                detalle.PrecioUnitario = request.PrecioUnitario;
                var error = await logica.ActualizarAsync(detalle);
                return error is null ? Results.Ok(detalle) : Results.BadRequest(error);
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

            // eliminar un detalle de pedido
            group.MapDelete("/{id:int}", async (int id, IDetallePedidosLogica logica) =>
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
        }

        private static async Task<bool> EsPedidoAccesibleAsync(
            int idPedido,
            IPedidosLogica pedidosLogica,
            ClaimsPrincipal usuarioActual)
        {
            var pedido = await pedidosLogica.ObtenerPorIdAsync(idPedido);
            return pedido is not null && usuarioActual.PuedeAcceder(pedido.IdUsuario);
        }
    }
}
