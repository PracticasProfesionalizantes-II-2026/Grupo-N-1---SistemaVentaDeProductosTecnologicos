using Totaltech.Entidades;
using Totaltech.Logica;

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

            group.MapPost("/", async (Compra compra, IComprasLogica logica) =>
            {
                await logica.CrearAsync(compra);
                return Results.Created($"/compras/{compra.IdCompra}", compra);
            });

            group.MapPut("/{id:int}", async (int id, Compra compra, IComprasLogica logica) =>
            {
                if (id != compra.IdCompra)
                {
                    return Results.BadRequest("El id de la URL no coincide con el id del body.");
                }

                var existente = await logica.ObtenerPorIdAsync(id);
                if (existente is null)
                {
                    return Results.NotFound();
                }

                await logica.ActualizarAsync(compra);
                return Results.NoContent();
            });

            group.MapDelete("/{id:int}", async (int id, IComprasLogica logica) =>
            {
                var compra = await logica.ObtenerPorIdAsync(id);
                if (compra is null)
                {
                    return Results.NotFound();
                }

                await logica.EliminarAsync(compra);
                return Results.NoContent();
            });
        }
    }
}
