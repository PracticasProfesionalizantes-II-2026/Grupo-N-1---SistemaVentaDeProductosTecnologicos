using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IReportesLogica : ILogica<Reporte>
    {
        Task<ReporteVentasDto> ObtenerVentasAsync();
        Task<ReporteIngresosDto> ObtenerIngresosAsync();
        Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync();
    }

    public class ReportesLogica : Logica<Reporte>, IReportesLogica
    {
        private readonly IReportesRepositorio _repositorio;

        public ReportesLogica(IReportesRepositorio repositorio) : base(repositorio)
        {
            _repositorio = repositorio;
        }

        public Task<ReporteVentasDto> ObtenerVentasAsync()
        {
            return _repositorio.ObtenerVentasAsync();
        }

        public Task<ReporteIngresosDto> ObtenerIngresosAsync()
        {
            return _repositorio.ObtenerIngresosAsync();
        }

        public Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync()
        {
            return _repositorio.ObtenerProductosMasVendidosAsync();
        }
    }
}
