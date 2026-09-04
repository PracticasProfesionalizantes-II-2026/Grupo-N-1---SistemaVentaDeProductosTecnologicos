using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

namespace Totaltech.Endpoints
{
    public static class DetalleCarritosEndpoints
    {
        public static void MapDetalleCarritosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/detallecarritos").WithTags("DetalleCarritos");

            // obtener todos los detalles de carritos--------------------------------
            group.MapGet("/", async (IDetalleCarritosLogica logica, ICarritosLogica carritosLogica, ClaimsPrincipal usuarioActual) =>
            {
                if (usuarioActual.EsAdministrador())
                {
                    return Results.Ok(await logica.ObtenerTodosAsync());
                }

                var idUsuario = usuarioActual.ObtenerIdUsuario();
                if (!idUsuario.HasValue)
                {
                    return Results.Unauthorized();
                }

                var carritos = await carritosLogica.ObtenerPorUsuarioAsync(idUsuario.Value);
                var detalles = new List<DetalleCarrito>();
                foreach (var carrito in carritos)
                {
                    detalles.AddRange(await logica.ObtenerPorCarritoAsync(carrito.IdCarrito));
                }

                return Results.Ok(detalles);
            }).RequireAuthorization();

            // obtener un detalle de carrito por su id--------------------------------
            group.MapGet("/{id:int}", async (int id, IDetalleCarritosLogica logica, ICarritosLogica carritosLogica, ClaimsPrincipal usuarioActual) =>
            {
                var detalle = await logica.ObtenerPorIdAsync(id);
                return detalle is null || !await EsCarritoAccesibleAsync(detalle.IdCarrito, carritosLogica, usuarioActual)
                    ? Results.NotFound()
                    : Results.Ok(detalle);
            }).RequireAuthorization();

            // obtener detalles de carrito por idCarrito--------------------------------
            group.MapGet("/carrito/{idCarrito:int}", async (int idCarrito, IDetalleCarritosLogica logica, ICarritosLogica carritosLogica, ClaimsPrincipal usuarioActual) =>
            {
                if (!await EsCarritoAccesibleAsync(idCarrito, carritosLogica, usuarioActual))
                {
                    return Results.NotFound();
                }

                var detalles = await logica.ObtenerPorCarritoAsync(idCarrito);
                return Results.Ok(detalles);
            }).RequireAuthorization();

            // crear un nuevo detalle de carrito--------------------------------
            group.MapPost("/", async (DetalleCarritoRequest request, IDetalleCarritosLogica logica, ICarritosLogica carritosLogica, ClaimsPrincipal usuarioActual) =>
            {
                if (!await EsCarritoAccesibleAsync(request.IdCarrito, carritosLogica, usuarioActual))
                {
                    return Results.NotFound();
                }

                var detalle = request.ToEntity();
                var error = await logica.CrearAsync(detalle);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/detallecarritos/{detalle.IdDetalleCarrito}", detalle);
            }).RequireAuthorization();

            // actualizar un detalle de carrito existente--------------------------------
            group.MapPut("/{id:int}", async (int id, DetalleCarritoRequest request, IDetalleCarritosLogica logica, ICarritosLogica carritosLogica, ClaimsPrincipal usuarioActual) =>
            {
                var detalle = await logica.ObtenerPorIdAsync(id);
                if (detalle is null ||
                    !await EsCarritoAccesibleAsync(detalle.IdCarrito, carritosLogica, usuarioActual) ||
                    !await EsCarritoAccesibleAsync(request.IdCarrito, carritosLogica, usuarioActual))
                {
                    return Results.NotFound();
                }

                detalle.IdCarrito = request.IdCarrito;
                detalle.IdProducto = request.IdProducto;
                detalle.Cantidad = request.Cantidad;
                detalle.PrecioUnitario = request.PrecioUnitario;
                var error = await logica.ActualizarAsync(detalle);
                return error is null ? Results.Ok(detalle) : Results.BadRequest(error);
            }).RequireAuthorization();

            // eliminar un detalle de carrito--------------------------------------------------------------------
            group.MapDelete("/{id:int}", async (int id, IDetalleCarritosLogica logica, ICarritosLogica carritosLogica, ClaimsPrincipal usuarioActual) =>
            {
                var detalle = await logica.ObtenerPorIdAsync(id);
                if (detalle is null || !await EsCarritoAccesibleAsync(detalle.IdCarrito, carritosLogica, usuarioActual))
                {
                    return Results.NotFound();
                }

                try
                {
                    var resultado = await logica.EliminarAsync(id);
                    if (resultado.Error is not null)
                    {
                        return Results.BadRequest(resultado.Error);
                    }

                    return resultado.Eliminado ? Results.NoContent() : Results.NotFound();
                }
                catch (DbUpdateException)
                {
                    return Results.Conflict("No se puede eliminar porque hay datos relacionados.");
                }
            }).RequireAuthorization();
        }

        private static async Task<bool> EsCarritoAccesibleAsync(
            int idCarrito,
            ICarritosLogica carritosLogica,
            ClaimsPrincipal usuarioActual)
        {
            var carrito = await carritosLogica.ObtenerPorIdAsync(idCarrito);
            return carrito is not null && usuarioActual.PuedeAcceder(carrito.IdUsuario);
        }
    }
}
