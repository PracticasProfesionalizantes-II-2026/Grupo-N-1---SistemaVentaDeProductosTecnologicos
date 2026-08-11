using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class CategoriasEndpoints
    {
        public static void MapCategoriasEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/categorias").WithTags("Categorias");

            // obtener todas las categorías
            group.MapGet("/", async (ICategoriasLogica logica) =>
            {
                var categorias = await logica.ObtenerTodosAsync();
                return Results.Ok(categorias);
            });

            // obtener una categoría por su id
            group.MapGet("/{id:int}", async (int id, ICategoriasLogica logica) =>
            {
                var categoria = await logica.ObtenerPorIdAsync(id);
                return categoria is null ? Results.NotFound() : Results.Ok(categoria);
            });

            // crear una nueva categoría
            group.MapPost("/", async (Categoria categoria, ICategoriasLogica logica) =>
            {
                var error = await logica.CrearAsync(categoria);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/categorias/{categoria.IdCategoria}", categoria);
            }); 

            // actualizar una categoría existente
            group.MapPut("/{id:int}", async (int id, Categoria categoria, ICategoriasLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                categoria.IdCategoria = id;
                var error = await logica.ActualizarAsync(categoria);
                return error is null ? Results.Ok(categoria) : Results.BadRequest(error);
            });

            // eliminar una categoría
            group.MapDelete("/{id:int}", async (int id, ICategoriasLogica logica) =>
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
