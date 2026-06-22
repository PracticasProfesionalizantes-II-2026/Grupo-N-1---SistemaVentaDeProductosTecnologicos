using Totaltech.Logica;
using Totaltech.Logica.DTOs;

namespace Totaltech.Endpoints
{
    public static class ReportesEndpoints
    {
        public static void MapReportesEndpoints(this WebApplication app)
        {
            // Estos endpoints traducen HTTP y delegan reglas de negocio a la capa de logica.
            var group = app.MapGroup("/reportes").WithTags("Reportes");

            group.MapGet("/", async (IReportesLogica logica) =>
            {
                var reportes = await logica.ObtenerTodosAsync();
                return Results.Ok(reportes.Select(reporte => reporte.ToResponse()));
            });

            group.MapGet("/{id:int}", async (int id, IReportesLogica logica) =>
            {
                var reporte = await logica.ObtenerPorIdAsync(id);
                return reporte is null ? Results.NotFound() : Results.Ok(reporte.ToResponse());
            });

            group.MapPost("/", async (CrearReporteRequest request, IReportesLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.CrearValidadoAsync(request.ToEntity());
                    return EndpointResults.FromResult(resultado, reporte => Results.Created($"/reportes/{reporte.IdReporte}", reporte.ToResponse()));
                });
            });

            group.MapPut("/{id:int}", async (int id, ActualizarReporteRequest request, IReportesLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.ActualizarValidadoAsync(id, request.ToEntity(id));
                    return EndpointResults.FromResult(resultado, reporte => Results.Ok(reporte.ToResponse()));
                });
            });

            group.MapDelete("/{id:int}", async (int id, IReportesLogica logica) =>
            {
                return await EndpointResults.HandleDbUpdateAsync(async () =>
                {
                    var resultado = await logica.EliminarPorIdAsync(id);
                    return EndpointResults.FromResult(resultado, () => Results.NoContent());
                });
            });

            group.MapGet("/ventas", async (IReportesLogica logica) =>
            {
                var reporte = await logica.ObtenerVentasAsync();
                return Results.Ok(reporte);
            });

            group.MapGet("/ingresos", async (IReportesLogica logica) =>
            {
                var reporte = await logica.ObtenerIngresosAsync();
                return Results.Ok(reporte);
            });

            group.MapGet("/productos-mas-vendidos", async (IReportesLogica logica) =>
            {
                var reporte = await logica.ObtenerProductosMasVendidosAsync();
                return Results.Ok(reporte);
            });
        }
    }
}
