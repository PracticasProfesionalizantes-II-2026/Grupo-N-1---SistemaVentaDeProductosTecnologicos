using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IReportesLogica : ILogica<Reporte>
    {
    }

    public class ReportesLogica : Logica<Reporte>, IReportesLogica
    {
        public ReportesLogica(IReportesRepositorio repositorio) : base(repositorio)
        {
        }
    }
}
