using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class CategoriasEndpoints
    {
        public static void MapCategoriasEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/categorias").WithTags("Categorias");

            group.MapGet("/", async (ICategoriasLogica logica) =>
            {
                var categorias = await logica.ObtenerTodosAsync();
                return Results.Ok(categorias);
            });

            group.MapGet("/{id:int}", async (int id, ICategoriasLogica logica) =>
            {
                var categoria = await logica.ObtenerPorIdAsync(id);
                return categoria is null ? Results.NotFound() : Results.Ok(categoria);
            });

            group.MapPost("/", async (Categoria categoria, ICategoriasLogica logica) =>
            {
                await logica.CrearAsync(categoria);
                return Results.Created($"/categorias/{categoria.IdCategoria}", categoria);
            });

            group.MapPut("/{id:int}", async (int id, Categoria categoria, ICategoriasLogica logica) =>
            {
                if (id != categoria.IdCategoria)
                {
                    return Results.BadRequest("El id de la URL no coincide con el id del body.");
                }

                var existente = await logica.ObtenerPorIdAsync(id);
                if (existente is null)
                {
                    return Results.NotFound();
                }

                await logica.ActualizarAsync(categoria);
                return Results.NoContent();
            });

            group.MapDelete("/{id:int}", async (int id, ICategoriasLogica logica) =>
            {
                var categoria = await logica.ObtenerPorIdAsync(id);
                if (categoria is null)
                {
                    return Results.NotFound();
                }

                await logica.EliminarAsync(categoria);
                return Results.NoContent();
            });
        }
    }
}
