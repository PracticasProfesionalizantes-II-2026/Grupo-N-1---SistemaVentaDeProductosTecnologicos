using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class DireccionesEndpoints
    {
        public static void MapDireccionesEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/direcciones").WithTags("Direcciones");
            // obtener todas las direcciones
            group.MapGet("/", async (IDireccionesLogica logica) =>
            {
                var direcciones = await logica.ObtenerTodosAsync();
                return Results.Ok(direcciones);
            });
            // obtener una direccion por su id
            group.MapGet("/{id:int}", async (int id, IDireccionesLogica logica) =>
            {
                var direccion = await logica.ObtenerPorIdAsync(id);
                return direccion is null ? Results.NotFound() : Results.Ok(direccion);
            });
            // crear una nueva direccion
            group.MapPost("/", async (DireccionRequest request, IDireccionesLogica logica) =>
            {
                var direccion = request.ToEntity();
                var error = await logica.CrearAsync(direccion);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/direcciones/{direccion.IdDireccion}", direccion);
            });
            // actualizar una direccion existente
            group.MapPut("/{id:int}", async (int id, DireccionRequest request, IDireccionesLogica logica) =>
            {
                var direccion = await logica.ObtenerPorIdAsync(id);
                if (direccion is null)
                {
                    return Results.NotFound();
                }

                AplicarCambios(direccion, request);
                var error = await logica.ActualizarAsync(direccion);
                return error is null ? Results.Ok(direccion) : Results.BadRequest(error);
            });
            // eliminar una direccion
            group.MapDelete("/{id:int}", async (int id, IDireccionesLogica logica) =>
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

        private static void AplicarCambios(Direccion direccion, DireccionRequest request)
        {
            direccion.IdUsuario = request.IdUsuario;
            direccion.Calle = request.Calle;
            direccion.Numero = request.Numero;
            direccion.Ciudad = request.Ciudad;
            direccion.Provincia = request.Provincia;
            direccion.CodigoPostal = request.CodigoPostal;
            direccion.Pais = request.Pais;
            direccion.Tipo = request.Tipo;
        }
    }
}
