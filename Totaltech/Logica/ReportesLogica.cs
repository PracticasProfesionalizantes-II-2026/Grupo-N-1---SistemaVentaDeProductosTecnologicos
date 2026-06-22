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
        private readonly IUsuariosRepositorio _usuariosRepositorio;

        public ReportesLogica(IReportesRepositorio repositorio, IUsuariosRepositorio usuariosRepositorio) : base(repositorio)
        {
            _repositorio = repositorio;
            _usuariosRepositorio = usuariosRepositorio;
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

        public override async Task<ResultadoOperacion<Reporte>> CrearValidadoAsync(Reporte reporte)
        {
            var validacion = await ValidarReporteAsync(reporte);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Reporte>.BadRequest(validacion.Error ?? "El reporte no es valido.");
            }

            return await base.CrearValidadoAsync(reporte);
        }

        public override async Task<ResultadoOperacion<Reporte>> ActualizarValidadoAsync(int id, Reporte reporte)
        {
            var validacion = await ValidarReporteAsync(reporte);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Reporte>.BadRequest(validacion.Error ?? "El reporte no es valido.");
            }

            return await base.ActualizarValidadoAsync(id, reporte);
        }

        private async Task<ResultadoOperacion> ValidarReporteAsync(Reporte reporte)
        {
            if (!await _usuariosRepositorio.ExisteAsync(reporte.IdUsuario))
            {
                return ResultadoOperacion.BadRequest("El usuario indicado no existe.");
            }

            if (reporte.FechaInicio != default && reporte.FechaFin != default && reporte.FechaInicio > reporte.FechaFin)
            {
                return ResultadoOperacion.BadRequest("La fecha de inicio no puede ser posterior a la fecha de fin.");
            }

            return ResultadoOperacion.Ok();
        }
    }
}
