using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

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

            group.MapPost("/", async (CategoriaRequest request, ICategoriasLogica logica) =>
            {
                var categoria = request.ToEntity();
                var error = await logica.CrearAsync(categoria);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/categorias/{categoria.IdCategoria}", categoria);
            });

            group.MapPut("/{id:int}", async (int id, CategoriaRequest request, ICategoriasLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                var categoria = request.ToEntity();
                categoria.IdCategoria = id;
                var error = await logica.ActualizarAsync(categoria);
                return error is null ? Results.Ok(categoria) : Results.BadRequest(error);
            });

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
