using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

namespace Totaltech.Endpoints
{
    public static class CarritosEndpoints
    {
        public static void MapCarritosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/carritos").WithTags("Carritos");

            // obtener todos los carritos
            group.MapGet("/", async (ICarritosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                var idUsuario = usuarioActual.ObtenerIdUsuario();
                if (!idUsuario.HasValue)
                {
                    return Results.Unauthorized();
                }

                var carritos = usuarioActual.EsAdministrador()
                    ? await logica.ObtenerTodosAsync()
                    : await logica.ObtenerPorUsuarioAsync(idUsuario.Value);
                return Results.Ok(carritos);
            }).RequireAuthorization();

            // obtener un carrito por su id
            group.MapGet("/{id:int}", async (int id, ICarritosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                var carrito = await logica.ObtenerPorIdAsync(id);
                return carrito is null || !usuarioActual.PuedeAcceder(carrito.IdUsuario)
                    ? Results.NotFound()
                    : Results.Ok(carrito);
            }).RequireAuthorization();

            // crear un nuevo carrito
            group.MapPost("/", async (CarritoRequest request, ICarritosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                if (!usuarioActual.EsAdministrador())
                {
                    request.IdUsuario = usuarioActual.ObtenerIdUsuario()!.Value;
                }

                var carrito = request.ToEntity();
                var error = await logica.CrearAsync(carrito);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/carritos/{carrito.IdCarrito}", carrito);
            }).RequireAuthorization();

            // actualizar un carrito existente
            group.MapPut("/{id:int}", async (int id, CarritoRequest request, ICarritosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                var carrito = await logica.ObtenerPorIdAsync(id);
                if (carrito is null || !usuarioActual.PuedeAcceder(carrito.IdUsuario))
                {
                    return Results.NotFound();
                }

                if (!usuarioActual.EsAdministrador())
                {
                    request.IdUsuario = carrito.IdUsuario;
                }

                carrito.IdUsuario = request.IdUsuario;
                carrito.FechaCreacion = request.FechaCreacion;
                carrito.Estado = request.Estado;
                var error = await logica.ActualizarAsync(carrito);
                return error is null ? Results.Ok(carrito) : Results.BadRequest(error);
            }).RequireAuthorization();

            // eliminar un carrito
            group.MapDelete("/{id:int}", async (int id, ICarritosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                var carrito = await logica.ObtenerPorIdAsync(id);
                if (carrito is null || !usuarioActual.PuedeAcceder(carrito.IdUsuario))
                {
                    return Results.NotFound();
                }

                try
                {
                    var eliminado = await logica.EliminarAsync(id);
                    return eliminado ? Results.NoContent() : Results.NotFound();
                }
                catch (DbUpdateException)
                {
                    return Results.Conflict("No se puede eliminar porque hay datos relacionados.");
                }
            }).RequireAuthorization();

            // obtener carritos por usuario
            group.MapGet("/usuario/{idUsuario:int}", async (int idUsuario, ICarritosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                if (!usuarioActual.PuedeAcceder(idUsuario))
                {
                    return Results.NotFound();
                }

                var carritos = await logica.ObtenerPorUsuarioAsync(idUsuario);
                return Results.Ok(carritos);
            }).RequireAuthorization();

            // agregar un producto a un carrito
            group.MapPost("/{idCarrito:int}/productos", async (int idCarrito, AgregarProductoCarritoDto request, ICarritosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                if (!await EsCarritoAccesibleAsync(idCarrito, logica, usuarioActual))
                {
                    return Results.NotFound();
                }

                var resultado = await logica.AgregarProductoAsync(idCarrito, request);
                if (resultado.Error is not null)
                {
                    return EsNoEncontrado(resultado.Error) ? Results.NotFound(resultado.Error) : Results.BadRequest(resultado.Error);
                }

                return Results.Created($"/detallecarritos/{resultado.Detalle!.IdDetalleCarrito}", resultado.Detalle);
            }).RequireAuthorization();

            // eliminar un producto de un carrito
            group.MapDelete("/{idCarrito:int}/productos/{idProducto:int}", async (int idCarrito, int idProducto, ICarritosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                if (!await EsCarritoAccesibleAsync(idCarrito, logica, usuarioActual))
                {
                    return Results.NotFound();
                }

                var error = await logica.EliminarProductoAsync(idCarrito, idProducto);
                if (error is not null)
                {
                    return EsNoEncontrado(error) ? Results.NotFound(error) : Results.BadRequest(error);
                }

                return Results.NoContent();
            }).RequireAuthorization();

            // confirmar un carrito y crear un pedido
            group.MapPost("/{idCarrito:int}/confirmar", async (int idCarrito, ConfirmarCarritoDto request, ICarritosLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                if (!await EsCarritoAccesibleAsync(idCarrito, logica, usuarioActual))
                {
                    return Results.NotFound();
                }

                var resultado = await logica.ConfirmarAsync(idCarrito, request);
                if (resultado.Error is not null)
                {
                    return EsNoEncontrado(resultado.Error) ? Results.NotFound(resultado.Error) : Results.BadRequest(resultado.Error);
                }

                return Results.Created($"/pedidos/{resultado.Pedido!.IdPedido}", resultado.Pedido);
            }).RequireAuthorization();
        }

        /// <summary>
        /// Verifica si el mensaje de error indica que el recurso no fue encontrado. CONSULTAR A GABI O EMI SI ESTO ES NECESARIO
        /// </summary>
        private static bool EsNoEncontrado(string error)
        {
            return error.Contains("no existe", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<bool> EsCarritoAccesibleAsync(
            int idCarrito,
            ICarritosLogica logica,
            ClaimsPrincipal usuarioActual)
        {
            var carrito = await logica.ObtenerPorIdAsync(idCarrito);
            return carrito is not null && usuarioActual.PuedeAcceder(carrito.IdUsuario);
        }
    }
}
