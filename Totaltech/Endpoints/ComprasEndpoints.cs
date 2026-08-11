using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class ComprasEndpoints
    {
        public static void MapComprasEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/compras").WithTags("Compras");

            group.MapGet("/", async (IComprasLogica logica) =>
            {
                var compras = await logica.ObtenerTodosAsync();
                return Results.Ok(compras);
            });

            group.MapGet("/{id:int}", async (int id, IComprasLogica logica) =>
            {
                var compra = await logica.ObtenerPorIdAsync(id);
                return compra is null ? Results.NotFound() : Results.Ok(compra);
            });

            group.MapPost("/", async (CompraRequest request, IComprasLogica logica) =>
            {
                var compra = request.ToEntity();
                var error = await logica.CrearAsync(compra);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/compras/{compra.IdCompra}", compra);
            });

            group.MapPut("/{id:int}", async (int id, CompraRequest request, IComprasLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                var compra = request.ToEntity();
                compra.IdCompra = id;
                var error = await logica.ActualizarAsync(compra);
                return error is null ? Results.Ok(compra) : Results.BadRequest(error);
            });

            group.MapDelete("/{id:int}", async (int id, IComprasLogica logica) =>
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
        }
    }
}
