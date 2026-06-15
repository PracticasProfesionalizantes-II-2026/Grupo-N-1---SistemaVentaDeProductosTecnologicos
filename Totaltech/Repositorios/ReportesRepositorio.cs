using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IReportesRepositorio : IRepositorio<Reporte>
    {
    }

    public class ReportesRepositorio : Repositorio<Reporte>, IReportesRepositorio
    {
        public ReportesRepositorio(TotaltechDbContext context) : base(context)
        {
        }
    }
}
