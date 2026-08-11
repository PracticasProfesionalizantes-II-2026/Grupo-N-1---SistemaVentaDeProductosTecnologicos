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


            // obtener todos los reportes--------------------------------
            group.MapGet("/", async (IReportesLogica logica) =>
            {
                var reportes = await logica.ObtenerTodosAsync();
                return Results.Ok(reportes);
            });

            // obtener un reporte por su id--------------------------------
            group.MapGet("/{id:int}", async (int id, IReportesLogica logica) =>
            {
                var reporte = await logica.ObtenerPorIdAsync(id);
                return reporte is null ? Results.NotFound() : Results.Ok(reporte);
            });

            // crear un nuevo reporte--------------------------------
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

            // actualizar un reporte existente--------------------------------
            group.MapPut("/{id:int}", async (int id, ReporteRequest request, IReportesLogica logica) =>
            {
                var reporte = await logica.ObtenerPorIdAsync(id);
                if (reporte is null)
                {
                    return Results.NotFound();
                }

                reporte.TipoReporte = request.TipoReporte;
                reporte.FechaInicio = request.FechaInicio;
                reporte.FechaFin = request.FechaFin;
                reporte.IdUsuario = request.IdUsuario;
                var error = await logica.ActualizarAsync(reporte);
                return error is null ? Results.Ok(reporte) : Results.BadRequest(error);
            });

            // eliminar un reporte--------------------------------------------------------------------
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

            // obtener ventas, ingresos y productos más vendidos--------------------------------
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
            //--------------------------------
        }
    }
}
