using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IProductosLogica
    {
        Task<List<Producto>> ObtenerTodosAsync();
        Task<Producto?> ObtenerPorIdAsync(int id);
        Task<string?> CrearAsync(Producto producto);
        Task<string?> ActualizarAsync(Producto producto);
        Task<bool> EliminarAsync(int id);
        Task<List<Producto>> BuscarAsync(string? texto);
        Task<List<Producto>> ObtenerPorCategoriaAsync(int idCategoria);
        Task<List<Producto>> ObtenerDisponiblesAsync();
        Task<bool> ActualizarStockAsync(int id, int stock);
    }

    public class ProductosLogica : IProductosLogica
    {
        private readonly IProductosRepositorio _repositorio;
        private readonly ICategoriasRepositorio _categoriasRepositorio;
        private readonly IProveedoresRepositorio _proveedoresRepositorio;

        public ProductosLogica(
            IProductosRepositorio repositorio,
            ICategoriasRepositorio categoriasRepositorio,
            IProveedoresRepositorio proveedoresRepositorio)
        {
            _repositorio = repositorio;
            _categoriasRepositorio = categoriasRepositorio;
            _proveedoresRepositorio = proveedoresRepositorio;
        }

        public Task<List<Producto>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<Producto?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<string?> CrearAsync(Producto producto)
        {
            var error = await ValidarProductoAsync(producto);
            if (error is not null)
            {
                return error;
            }

            await _repositorio.CrearAsync(producto);
            return null;
        }

        public async Task<string?> ActualizarAsync(Producto producto)
        {
            var error = await ValidarProductoAsync(producto);
            if (error is not null)
            {
                return error;
            }

            await _repositorio.ActualizarAsync(producto);
            return null;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var producto = await _repositorio.ObtenerPorIdAsync(id);
            if (producto is null)
            {
                return false;
            }

            await _repositorio.EliminarAsync(producto);
            return true;
        }

        public Task<List<Producto>> BuscarAsync(string? texto)
        {
            return _repositorio.BuscarAsync(texto);
        }

        public Task<List<Producto>> ObtenerPorCategoriaAsync(int idCategoria)
        {
            return _repositorio.ObtenerPorCategoriaAsync(idCategoria);
        }

        public Task<List<Producto>> ObtenerDisponiblesAsync()
        {
            return _repositorio.ObtenerDisponiblesAsync();
        }

        public async Task<bool> ActualizarStockAsync(int id, int stock)
        {
            var producto = await _repositorio.ObtenerPorIdAsync(id);
            if (producto is null)
            {
                return false;
            }

            producto.Stock = stock;
            await _repositorio.ActualizarAsync(producto);
            return true;
        }

        private async Task<string?> ValidarProductoAsync(Producto producto)
        {
            if (string.IsNullOrWhiteSpace(producto.Nombre))
            {
                return "El nombre del producto es obligatorio.";
            }

            if (producto.Precio < 0 || producto.Stock < 0)
            {
                return "El precio y el stock no pueden ser negativos.";
            }

            if (!await _categoriasRepositorio.ExisteAsync(producto.IdCategoria))
            {
                return "La categoria indicada no existe.";
            }

            if (!await _proveedoresRepositorio.ExisteAsync(producto.IdProveedor))
            {
                return "El proveedor indicado no existe.";
            }

            return null;
        }
    }
}
