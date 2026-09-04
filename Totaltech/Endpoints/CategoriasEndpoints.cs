using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

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
            }).AllowAnonymous();

            // obtener una categoría por su id
            group.MapGet("/{id:int}", async (int id, ICategoriasLogica logica) =>
            {
                var categoria = await logica.ObtenerPorIdAsync(id);
                return categoria is null ? Results.NotFound() : Results.Ok(categoria);
            }).AllowAnonymous();

            // crear una nueva categoría
            group.MapPost("/", async (CategoriaRequest request, ICategoriasLogica logica) =>
            {
                var categoria = request.ToEntity();
                var error = await logica.CrearAsync(categoria);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/categorias/{categoria.IdCategoria}", categoria);
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

            // actualizar una categoría existente
            group.MapPut("/{id:int}", async (int id, CategoriaRequest request, ICategoriasLogica logica) =>
            {
                var categoria = await logica.ObtenerPorIdAsync(id);
                if (categoria is null)
                {
                    return Results.NotFound();
                }

                categoria.Nombre = request.Nombre;
                categoria.Descripcion = request.Descripcion;
                var error = await logica.ActualizarAsync(categoria);
                return error is null ? Results.Ok(categoria) : Results.BadRequest(error);
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

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
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);
        }
    }
}
