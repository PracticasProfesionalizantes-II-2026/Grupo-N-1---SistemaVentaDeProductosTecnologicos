using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class DetalleCarritosEndpoints
    {
        public static void MapDetalleCarritosEndpoints(this WebApplication app)
        {
            var group = app.MapCrud<DetalleCarrito, IDetalleCarritosLogica>("/detallecarritos", "Detalle carritos", detalle => detalle.IdDetalleCarrito);

            group.MapGet("/carrito/{idCarrito:int}", async (int idCarrito, IDetalleCarritosLogica logica) =>
            {
                var detalles = await logica.ObtenerPorCarritoAsync(idCarrito);
                return Results.Ok(detalles);
            });
        }
    }
}
