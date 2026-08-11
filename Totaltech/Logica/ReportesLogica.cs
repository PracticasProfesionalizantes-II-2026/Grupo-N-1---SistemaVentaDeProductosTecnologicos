using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IReportesLogica
    {
        Task<List<Reporte>> ObtenerTodosAsync();
        Task<Reporte?> ObtenerPorIdAsync(int id);
        Task<string?> CrearAsync(Reporte reporte);
        Task<string?> ActualizarAsync(Reporte reporte);
        Task<bool> EliminarAsync(int id);
        Task<ReporteVentasDto> ObtenerVentasAsync();
        Task<ReporteIngresosDto> ObtenerIngresosAsync();
        Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync();
    }

    public class ReportesLogica : IReportesLogica
    {
        private readonly IReportesRepositorio _repositorio;
        private readonly IUsuariosRepositorio _usuariosRepositorio;

        public ReportesLogica(IReportesRepositorio repositorio, IUsuariosRepositorio usuariosRepositorio)
        {
            _repositorio = repositorio;
            _usuariosRepositorio = usuariosRepositorio;
        }

        public Task<List<Reporte>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<Reporte?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<string?> CrearAsync(Reporte reporte)
        {
            var error = await ValidarReporteAsync(reporte);
            if (error is not null)
            {
                return error;
            }

            await _repositorio.CrearAsync(reporte);
            return null;
        }

        public async Task<string?> ActualizarAsync(Reporte reporte)
        {
            var error = await ValidarReporteAsync(reporte);
            if (error is not null)
            {
                return error;
            }

            await _repositorio.ActualizarAsync(reporte);
            return null;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var reporte = await _repositorio.ObtenerPorIdAsync(id);
            if (reporte is null)
            {
                return false;
            }

            await _repositorio.EliminarAsync(reporte);
            return true;
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

        private async Task<string?> ValidarReporteAsync(Reporte reporte)
        {
            if (!Enum.IsDefined(reporte.TipoReporte))
            {
                return "El tipo de reporte no es valido.";
            }

            if (!await _usuariosRepositorio.ExisteAsync(reporte.IdUsuario))
            {
                return "El usuario indicado no existe.";
            }

            if (reporte.FechaInicio != default && reporte.FechaFin != default && reporte.FechaInicio > reporte.FechaFin)
            {
                return "La fecha de inicio no puede ser posterior a la fecha de fin.";
            }

            return null;
        }
    }
}
