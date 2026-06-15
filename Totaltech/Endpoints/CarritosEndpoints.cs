using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class CarritosEndpoints
    {
        public static void MapCarritosEndpoints(this WebApplication app)
        {
            var group = app.MapCrud<Carrito, ICarritosLogica>("/carritos", "Carritos", carrito => carrito.IdCarrito);

            group.MapGet("/usuario/{idUsuario:int}", async (int idUsuario, ICarritosLogica logica) =>
            {
                var carritos = await logica.ObtenerPorUsuarioAsync(idUsuario);
                return Results.Ok(carritos);
            });

            group.MapPost("/{idCarrito:int}/productos", async (int idCarrito, AgregarProductoCarritoDto request, ICarritosLogica logica) =>
            {
                if (request.Cantidad <= 0)
                {
                    return Results.BadRequest("La cantidad debe ser mayor a cero.");
                }

                var detalle = await logica.AgregarProductoAsync(idCarrito, request);

                if (detalle is null)
                {
                    return Results.NotFound();
                }

                return Results.Created($"/detallecarritos/{detalle.IdDetalleCarrito}", detalle);
            });

            group.MapDelete("/{idCarrito:int}/productos/{idProducto:int}", async (int idCarrito, int idProducto, ICarritosLogica logica) =>
            {
                var eliminado = await logica.EliminarProductoAsync(idCarrito, idProducto);
                return eliminado ? Results.NoContent() : Results.NotFound();
            });
        }
    }
}
