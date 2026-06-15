using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class ReportesEndpoints
    {
        public static void MapReportesEndpoints(this WebApplication app)
        {
            app.MapCrud<Reporte, IReportesLogica>("/reportes", "Reportes", reporte => reporte.IdReporte);
        }
    }
}
