using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IComprasLogica
    {
        Task<List<Compra>> ObtenerTodosAsync();
        Task<Compra?> ObtenerPorIdAsync(int id);
        Task<string?> CrearAsync(Compra compra);
        Task<string?> ActualizarAsync(Compra compra);
        Task<bool> EliminarAsync(int id);
    }

    public class ComprasLogica : IComprasLogica
    {
        private readonly IComprasRepositorio _repositorio;
        private readonly IProveedoresRepositorio _proveedoresRepositorio;

        public ComprasLogica(IComprasRepositorio repositorio, IProveedoresRepositorio proveedoresRepositorio)
        {
            _repositorio = repositorio;
            _proveedoresRepositorio = proveedoresRepositorio;
        }

        public Task<List<Compra>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<Compra?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<string?> CrearAsync(Compra compra)
        {
            var error = await ValidarCompraAsync(compra);
            if (error is not null)
            {
                return error;
            }

            if (compra.FechaCompra == default)
            {
                compra.FechaCompra = DateTime.Now;
            }

            await _repositorio.CrearAsync(compra);
            return null;
        }

        public async Task<string?> ActualizarAsync(Compra compra)
        {
            var error = await ValidarCompraAsync(compra);
            if (error is not null)
            {
                return error;
            }

            if (compra.FechaCompra == default)
            {
                compra.FechaCompra = DateTime.Now;
            }

            await _repositorio.ActualizarAsync(compra);
            return null;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var compra = await _repositorio.ObtenerPorIdAsync(id);
            if (compra is null)
            {
                return false;
            }

            await _repositorio.EliminarAsync(compra);
            return true;
        }

        private async Task<string?> ValidarCompraAsync(Compra compra)
        {
            if (!Enum.IsDefined(compra.Estado))
            {
                return "El estado de la compra no es valido.";
            }

            if (compra.Total < 0)
            {
                return "El total de la compra no puede ser negativo.";
            }

            if (!await _proveedoresRepositorio.ExisteAsync(compra.IdProveedor))
            {
                return "El proveedor indicado no existe.";
            }

            return null;
        }
    }
}
