using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IProveedoresLogica
    {
        Task<List<Proveedor>> ObtenerTodosAsync();
        Task<Proveedor?> ObtenerPorIdAsync(int id);
        Task<string?> CrearAsync(Proveedor proveedor);
        Task<string?> ActualizarAsync(Proveedor proveedor);
        Task<bool> EliminarAsync(int id);
    }

    public class ProveedoresLogica : IProveedoresLogica
    {
        private readonly IProveedoresRepositorio _repositorio;

        public ProveedoresLogica(IProveedoresRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public Task<List<Proveedor>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<Proveedor?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<string?> CrearAsync(Proveedor proveedor)
        {
            var error = ValidarProveedor(proveedor);
            if (error is not null)
            {
                return error;
            }

            await _repositorio.CrearAsync(proveedor);
            return null;
        }

        public async Task<string?> ActualizarAsync(Proveedor proveedor)
        {
            var error = ValidarProveedor(proveedor);
            if (error is not null)
            {
                return error;
            }

            await _repositorio.ActualizarAsync(proveedor);
            return null;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var proveedor = await _repositorio.ObtenerPorIdAsync(id);
            if (proveedor is null)
            {
                return false;
            }

            await _repositorio.EliminarAsync(proveedor);
            return true;
        }

        private static string? ValidarProveedor(Proveedor proveedor)
        {
            if (string.IsNullOrWhiteSpace(proveedor.RazonSocial) || string.IsNullOrWhiteSpace(proveedor.Cuit))
            {
                return "La razon social y el CUIT son obligatorios.";
            }

            if (proveedor.Cuit.Length > 20)
            {
                return "El CUIT no puede superar los 20 caracteres.";
            }

            if (string.IsNullOrWhiteSpace(proveedor.EmailComercial)
                || string.IsNullOrWhiteSpace(proveedor.CondicionIva)
                || string.IsNullOrWhiteSpace(proveedor.MonedaPreferida))
            {
                return "El email comercial, la condicion de IVA y la moneda preferida son obligatorios.";
            }

            if (proveedor.PlazoPagoDias < 0 || proveedor.TiempoEntregaDias < 0)
            {
                return "Los plazos no pueden ser negativos.";
            }

            if (proveedor.Direccion is null)
            {
                return "La direccion del proveedor es obligatoria.";
            }

            if (string.IsNullOrWhiteSpace(proveedor.Direccion.Calle)
                || string.IsNullOrWhiteSpace(proveedor.Direccion.Numero))
            {
                return "La calle y el numero de la direccion son obligatorios.";
            }

            return null;
        }
    }
}
