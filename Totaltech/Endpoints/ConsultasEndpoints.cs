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
                await logica.CrearAsync(consulta);
                return Results.Created($"/consultas/{consulta.IdConsulta}", consulta);
            });

            group.MapPut("/{id:int}", async (int id, Consulta consulta, IConsultasLogica logica) =>
            {
                if (id != consulta.IdConsulta)
                {
                    return Results.BadRequest("El id de la URL no coincide con el id del body.");
                }

                var existente = await logica.ObtenerPorIdAsync(id);
                if (existente is null)
                {
                    return Results.NotFound();
                }

                await logica.ActualizarAsync(consulta);
                return Results.NoContent();
            });

            group.MapDelete("/{id:int}", async (int id, IConsultasLogica logica) =>
            {
                var consulta = await logica.ObtenerPorIdAsync(id);
                if (consulta is null)
                {
                    return Results.NotFound();
                }

                await logica.EliminarAsync(consulta);
                return Results.NoContent();
            });
        }
    }
}
