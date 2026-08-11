using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;
using Totaltech.Logica;
using Totaltech.Logica.DTOs;

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

            group.MapPost("/", async (ReporteRequest request, IReportesLogica logica) =>
            {
                var reporte = request.ToEntity();
                var error = await logica.CrearAsync(reporte);
                if (error is not null)
                {
                    return Results.BadRequest(error);
                }

                return Results.Created($"/reportes/{reporte.IdReporte}", reporte);
            });

            group.MapPut("/{id:int}", async (int id, ReporteRequest request, IReportesLogica logica) =>
            {
                if (await logica.ObtenerPorIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                var reporte = request.ToEntity();
                reporte.IdReporte = id;
                var error = await logica.ActualizarAsync(reporte);
                return error is null ? Results.Ok(reporte) : Results.BadRequest(error);
            });

            group.MapDelete("/{id:int}", async (int id, IReportesLogica logica) =>
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
