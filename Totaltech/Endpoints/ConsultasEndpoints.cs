using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;

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
            });

            group.MapGet("/{id:int}", async (int id, IConsultasLogica logica) =>
            {
                var consulta = await logica.ObtenerPorIdAsync(id);
                return consulta is null ? Results.NotFound() : Results.Ok(consulta);
            });

            group.MapGet("/usuario/{idUsuario:int}", async (int idUsuario, IConsultasLogica logica) =>
            {
                var consultas = await logica.ObtenerPorUsuarioAsync(idUsuario);
                return Results.Ok(consultas);
            });

            group.MapPost("/", async (Consulta consulta, IConsultasLogica logica) =>
            {
                var error = await logica.CrearAsync(consulta);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/consultas/{consulta.IdConsulta}", consulta);
            });

            group.MapPut("/{id:int}", async (int id, Consulta consulta, IConsultasLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                consulta.IdConsulta = id;
                var error = await logica.ActualizarAsync(consulta);
                return error is null ? Results.Ok(consulta) : Results.BadRequest(error);
            });

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
            });
        }
    }
}
