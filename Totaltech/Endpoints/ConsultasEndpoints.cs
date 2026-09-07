using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;
using Totaltech.Seguridad;

namespace Totaltech.Endpoints
{
    public static class ConsultasEndpoints
    {
        public static void MapConsultasEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/consultas").WithTags("Consultas");

            group.MapGet("/", async (IConsultasLogica logica) =>
            {
                var consultas = await logica.ObtenerTodosAsync();
                return Results.Ok(consultas);
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

            group.MapGet("/{id:int}", async (int id, IConsultasLogica logica) =>
            {
                var consulta = await logica.ObtenerPorIdAsync(id);
                return consulta is null ? Results.NotFound() : Results.Ok(consulta);
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

            group.MapGet("/usuario/{idUsuario:int}", async (int idUsuario, IConsultasLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                if (!usuarioActual.PuedeAcceder(idUsuario))
                {
                    return Results.NotFound();
                }

                var consultas = await logica.ObtenerPorUsuarioAsync(idUsuario);
                return Results.Ok(consultas);
            }).RequireAuthorization();

            group.MapPost("/", async (ConsultaRequest request, IConsultasLogica logica, ClaimsPrincipal usuarioActual) =>
            {
                request.IdUsuario = usuarioActual.Identity?.IsAuthenticated == true
                    ? usuarioActual.ObtenerIdUsuario()
                    : null;

                var consulta = request.ToEntity();
                var error = await logica.CrearAsync(consulta);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/consultas/{consulta.IdConsulta}", consulta);
            }).AllowAnonymous();

            group.MapPut("/{id:int}", async (int id, ConsultaRequest request, IConsultasLogica logica) =>
            {
                var consulta = await logica.ObtenerPorIdAsync(id);
                if (consulta is null)
                {
                    return Results.NotFound();
                }

                consulta.IdUsuario = request.IdUsuario;
                consulta.Nombre = request.Nombre;
                consulta.Email = request.Email;
                consulta.Mensaje = request.Mensaje;
                consulta.FechaConsulta = request.FechaConsulta;
                consulta.Estado = request.Estado;
                var error = await logica.ActualizarAsync(consulta);
                return error is null ? Results.Ok(consulta) : Results.BadRequest(error);
            }).RequireAuthorization(Autorizacion.PoliticaAdministrador);

            group.MapDelete("/{id:int}", async (int id, IConsultasLogica logica) =>
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
