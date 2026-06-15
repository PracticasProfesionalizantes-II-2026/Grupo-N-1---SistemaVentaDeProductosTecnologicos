using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class PagosEndpoints
    {
        public static void MapPagosEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/pagos").WithTags("Pagos");

            group.MapGet("/", async (IPagosLogica logica) =>
            {
                var pagos = await logica.ObtenerTodosAsync();
                return Results.Ok(pagos);
            });

            group.MapGet("/{id:int}", async (int id, IPagosLogica logica) =>
            {
                var pago = await logica.ObtenerPorIdAsync(id);
                return pago is null ? Results.NotFound() : Results.Ok(pago);
            });

            group.MapPost("/", async (Pago pago, IPagosLogica logica) =>
            {
                await logica.CrearAsync(pago);
                return Results.Created($"/pagos/{pago.IdPago}", pago);
            });

            group.MapPut("/{id:int}", async (int id, Pago pago, IPagosLogica logica) =>
            {
                if (id != pago.IdPago)
                {
                    return Results.BadRequest("El id de la URL no coincide con el id del body.");
                }

                var existente = await logica.ObtenerPorIdAsync(id);
                if (existente is null)
                {
                    return Results.NotFound();
                }

                await logica.ActualizarAsync(pago);
                return Results.NoContent();
            });

            group.MapDelete("/{id:int}", async (int id, IPagosLogica logica) =>
            {
                var pago = await logica.ObtenerPorIdAsync(id);
                if (pago is null)
                {
                    return Results.NotFound();
                }

                await logica.EliminarAsync(pago);
                return Results.NoContent();
            });

            group.MapPatch("/{id:int}/estado", async (int id, ActualizarEstadoPagoRequest request, IPagosLogica logica) =>
            {
                var actualizado = await logica.ActualizarEstadoAsync(id, request.Estado);
                return actualizado ? Results.NoContent() : Results.NotFound();
            });
        }
    }
}
