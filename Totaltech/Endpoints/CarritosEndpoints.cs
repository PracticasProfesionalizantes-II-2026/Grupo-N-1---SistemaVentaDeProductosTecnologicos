using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class CarritosEndpoints
    {
        public static void MapCarritosEndpoints(this WebApplication app)
        {
            // Estos endpoints traducen HTTP y delegan reglas de negocio a la capa de logica.
            var group = app.MapGroup("/carritos").WithTags("Carritos");

            group.MapGet("/", async (ICarritosLogica logica) =>
            {
                var carritos = await logica.ObtenerTodosAsync();
                return Results.Ok(carritos.Select(carrito => carrito.ToResponse()));
            });

            group.MapGet("/{id:int}", async (int id, ICarritosLogica logica) =>
            {
                var carrito = await logica.ObtenerPorIdAsync(id);
                return carrito is null ? Results.NotFound() : Results.Ok(carrito.ToResponse());
            });

            group.MapPost("/", async (CrearCarritoRequest request, ICarritosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearValidadoAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, carrito => Results.Created($"/carritos/{carrito.IdCarrito}", carrito.ToResponse()));
                });
            });

            group.MapPut("/{id:int}", async (int id, ActualizarCarritoRequest request, ICarritosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarValidadoAsync(id, request.ToEntity(id));
                    return EndpointResults.FromResult(resultado, carrito => Results.Ok(carrito.ToResponse()));
                });
            });

            group.MapDelete("/{id:int}", async (int id, ICarritosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.EliminarPorIdAsync(id);
                    return EndpointResults.FromResult(resultado, () => Results.NoContent());
                });
            });

            group.MapGet("/usuario/{idUsuario:int}", async (int idUsuario, ICarritosLogica logica) =>
            {
                var carritos = await logica.ObtenerPorUsuarioAsync(idUsuario);
                return Results.Ok(carritos.Select(carrito => carrito.ToResponse()));
            });

            group.MapPost("/{idCarrito:int}/productos", async (int idCarrito, AgregarProductoCarritoDto request, ICarritosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.AgregarProductoAsync(idCarrito, request);
                    return EndpointResults.FromResult(resultado, detalle => Results.Created($"/detallecarritos/{detalle.IdDetalleCarrito}", detalle.ToResponse()));
                });
            });

            group.MapDelete("/{idCarrito:int}/productos/{idProducto:int}", async (int idCarrito, int idProducto, ICarritosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.EliminarProductoAsync(idCarrito, idProducto);
                    return EndpointResults.FromResult(resultado, () => Results.NoContent());
                });
            });

            group.MapPost("/{idCarrito:int}/confirmar", async (int idCarrito, ConfirmarCarritoDto request, ICarritosLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ConfirmarAsync(idCarrito, request);
                    return EndpointResults.FromResult(resultado, pedido => Results.Created($"/pedidos/{pedido.IdPedido}", pedido.ToResponse()));
                });
            });
        }
    }
}
