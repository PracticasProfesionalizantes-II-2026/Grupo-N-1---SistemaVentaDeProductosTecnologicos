using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class ReportesEndpoints
    {
        public static void MapReportesEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/reportes").WithTags("Reportes");

            group.MapGet("/", async (IReportesLogica logica) =>
            {
                var reportes = await logica.ObtenerTodosAsync();
                return Results.Ok(reportes);
            });

            group.MapGet("/{id:int}", async (int id, IReportesLogica logica) =>
            {
                var reporte = await logica.ObtenerPorIdAsync(id);
                return reporte is null ? Results.NotFound() : Results.Ok(reporte);
            });

            group.MapPost("/", async (Reporte reporte, IReportesLogica logica) =>
            {
                await logica.CrearAsync(reporte);
                return Results.Created($"/reportes/{reporte.IdReporte}", reporte);
            });

            group.MapPut("/{id:int}", async (int id, Reporte reporte, IReportesLogica logica) =>
            {
                if (id != reporte.IdReporte)
                {
                    return Results.BadRequest("El id de la URL no coincide con el id del body.");
                }

                var existente = await logica.ObtenerPorIdAsync(id);
                if (existente is null)
                {
                    return Results.NotFound();
                }

                await logica.ActualizarAsync(reporte);
                return Results.NoContent();
            });

            group.MapDelete("/{id:int}", async (int id, IReportesLogica logica) =>
            {
                var reporte = await logica.ObtenerPorIdAsync(id);
                if (reporte is null)
                {
                    return Results.NotFound();
                }

                await logica.EliminarAsync(reporte);
                return Results.NoContent();
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
