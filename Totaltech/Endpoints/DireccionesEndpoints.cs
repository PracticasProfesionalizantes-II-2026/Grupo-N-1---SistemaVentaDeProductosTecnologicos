using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

namespace Totaltech.Endpoints
{
    public static class DireccionesEndpoints
    {
        public static void MapDireccionesEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/direcciones").WithTags("Direcciones");
            // obtener todas las direcciones
            group.MapGet("/", async (IDireccionesLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                var idUsuario = usuarioActual.ObtenerIdUsuario();
                if (!idUsuario.HasValue)
                {
                    return Results.Unauthorized();
                }

                var direcciones = usuarioActual.EsAdministrador()
                    ? await logica.ObtenerTodosAsync()
                    : await logica.ObtenerPorUsuarioAsync(idUsuario.Value);
                return Results.Ok(direcciones);
            }).RequireAuthorization();
            // obtener una direccion por su id
            group.MapGet("/{id:int}", async (int id, IDireccionesLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                var direccion = await logica.ObtenerPorIdAsync(id);
                return direccion is null || !usuarioActual.PuedeAcceder(direccion.IdUsuario)
                    ? Results.NotFound()
                    : Results.Ok(direccion);
            }).RequireAuthorization();
            // crear una nueva direccion
            group.MapPost("/", async (DireccionRequest request, IDireccionesLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                if (!usuarioActual.EsAdministrador())
                {
                    request.IdUsuario = usuarioActual.ObtenerIdUsuario();
                }

                var direccion = request.ToEntity();
                var error = await logica.CrearAsync(direccion);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/direcciones/{direccion.IdDireccion}", direccion);
            }).RequireAuthorization();
            // actualizar una direccion existente
            group.MapPut("/{id:int}", async (int id, DireccionRequest request, IDireccionesLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                var direccion = await logica.ObtenerPorIdAsync(id);
                if (direccion is null || !usuarioActual.PuedeAcceder(direccion.IdUsuario))
                {
                    return Results.NotFound();
                }

                if (!usuarioActual.EsAdministrador())
                {
                    request.IdUsuario = usuarioActual.ObtenerIdUsuario();
                }

                AplicarCambios(direccion, request);
                var error = await logica.ActualizarAsync(direccion);
                return error is null ? Results.Ok(direccion) : Results.BadRequest(error);
            }).RequireAuthorization();
            // eliminar una direccion
            group.MapDelete("/{id:int}", async (int id, IDireccionesLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                var direccion = await logica.ObtenerPorIdAsync(id);
                if (direccion is null || !usuarioActual.PuedeAcceder(direccion.IdUsuario))
                {
                    return Results.NotFound();
                }

                try
                {
                    var eliminado = await logica.EliminarAsync(id);
                    return eliminado ? Results.NoContent() : Results.NotFound();
                }
                catch (DbUpdateException)
                {
                    return Results.Conflict("No se puede eliminar porque hay datos relacionados.");
                }
            }).RequireAuthorization();
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
