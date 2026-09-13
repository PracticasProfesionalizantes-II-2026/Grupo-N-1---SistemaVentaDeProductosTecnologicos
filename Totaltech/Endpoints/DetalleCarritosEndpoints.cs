using System.Security.Claims;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Seguridad;

namespace Totaltech.Endpoints;

public static class DetalleCarritosEndpoints
{
    public static void MapDetalleCarritosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/detallecarritos").WithTags("DetalleCarritos");

        group.MapGet("/", async (
            IDetalleCarritosLogica logica,
            ICarritosLogica carritosLogica,
            ClaimsPrincipal usuarioActual) =>
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

        group.MapGet("/{id:int}", async (
            int id,
            IDetalleCarritosLogica logica,
            ICarritosLogica carritosLogica,
            ClaimsPrincipal usuarioActual) =>
        {
            var detalle = await logica.ObtenerPorIdAsync(id);
            return detalle is null ||
                   !await EsCarritoAccesibleAsync(detalle.IdCarrito, carritosLogica, usuarioActual)
                ? Results.NotFound()
                : Results.Ok(detalle);
        }).RequireAuthorization();

        group.MapGet("/carrito/{idCarrito:int}", async (
            int idCarrito,
            IDetalleCarritosLogica logica,
            ICarritosLogica carritosLogica,
            ClaimsPrincipal usuarioActual) =>
        {
            if (!await EsCarritoAccesibleAsync(idCarrito, carritosLogica, usuarioActual))
            {
                return Results.NotFound();
            }

            return Results.Ok(await logica.ObtenerPorCarritoAsync(idCarrito));
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
