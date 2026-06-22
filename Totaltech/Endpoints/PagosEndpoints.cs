using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

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
                var error = await logica.CrearAsync(pago);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/pagos/{pago.IdPago}", pago);
            });

            group.MapPut("/{id:int}", async (int id, Pago pago, IPagosLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                pago.IdPago = id;
                var error = await logica.ActualizarAsync(pago);
                return error is null ? Results.Ok(pago) : Results.BadRequest(error);
            });

            group.MapDelete("/{id:int}", async (int id, IPagosLogica logica) =>
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
            });

            group.MapPatch("/{id:int}/estado", async (int id, ActualizarEstadoPagoRequest request, IPagosLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                var error = await logica.ActualizarEstadoAsync(id, request.Estado);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                var pago = await logica.ObtenerPorIdAsync(id);
                return Results.Ok(pago);
            });
        }
    }
}
