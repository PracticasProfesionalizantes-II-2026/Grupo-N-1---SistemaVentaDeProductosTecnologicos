using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class ConsultasEndpoints
    {
        public static void MapConsultasEndpoints(this WebApplication app)
        {
            // Estos endpoints traducen HTTP y delegan reglas de negocio a la capa de logica.
            var group = app.MapGroup("/consultas").WithTags("Consultas");

            group.MapGet("/", async (IConsultasLogica logica) =>
            {
                var consultas = await logica.ObtenerTodosAsync();
                return Results.Ok(consultas.Select(consulta => consulta.ToResponse()));
            });

            group.MapGet("/{id:int}", async (int id, IConsultasLogica logica) =>
            {
                var consulta = await logica.ObtenerPorIdAsync(id);
                return consulta is null ? Results.NotFound() : Results.Ok(consulta.ToResponse());
            });

            group.MapGet("/usuario/{idUsuario:int}", async (int idUsuario, IConsultasLogica logica) =>
            {
                var consultas = await logica.ObtenerPorUsuarioAsync(idUsuario);
                return Results.Ok(consultas.Select(consulta => consulta.ToResponse()));
            });

            group.MapPost("/", async (CrearConsultaRequest request, IConsultasLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearValidadoAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, consulta => Results.Created($"/consultas/{consulta.IdConsulta}", consulta.ToResponse()));
                });
            });

            group.MapPut("/{id:int}", async (int id, ActualizarConsultaRequest request, IConsultasLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarValidadoAsync(id, request.ToEntity(id));
                    return EndpointResults.FromResult(resultado, consulta => Results.Ok(consulta.ToResponse()));
                });
            });

            group.MapDelete("/{id:int}", async (int id, IConsultasLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.EliminarPorIdAsync(id);
                    return EndpointResults.FromResult(resultado, () => Results.NoContent());
                });
            });
        }
    }
}
