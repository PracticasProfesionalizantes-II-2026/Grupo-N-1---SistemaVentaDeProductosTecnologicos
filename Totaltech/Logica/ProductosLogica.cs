using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IProductosLogica : ILogica<Producto>
    {
        Task<List<Producto>> BuscarAsync(string? texto);
        Task<List<Producto>> ObtenerPorCategoriaAsync(int idCategoria);
        Task<List<Producto>> ObtenerDisponiblesAsync();
        Task<bool> ActualizarStockAsync(int id, int stock);
    }

    public class ProductosLogica : Logica<Producto>, IProductosLogica
    {
        private readonly IProductosRepositorio _repositorio;
        private readonly ICategoriasRepositorio _categoriasRepositorio;
        private readonly IProveedoresRepositorio _proveedoresRepositorio;

        public ProductosLogica(IProductosRepositorio repositorio, ICategoriasRepositorio categoriasRepositorio, IProveedoresRepositorio proveedoresRepositorio) : base(repositorio)
        {
            _repositorio = repositorio;
            _categoriasRepositorio = categoriasRepositorio;
            _proveedoresRepositorio = proveedoresRepositorio;
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

        public override async Task<ResultadoOperacion<Producto>> CrearValidadoAsync(Producto producto)
        {
            var validacion = await ValidarProductoAsync(producto);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Producto>.BadRequest(validacion.Error ?? "El producto no es valido.");
            }

            return await base.CrearValidadoAsync(producto);
        }

        public override async Task<ResultadoOperacion<Producto>> ActualizarValidadoAsync(int id, Producto producto)
        {
            var validacion = await ValidarProductoAsync(producto);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Producto>.BadRequest(validacion.Error ?? "El producto no es valido.");
            }

            return await base.ActualizarValidadoAsync(id, producto);
        }

        private async Task<ResultadoOperacion> ValidarProductoAsync(Producto producto)
        {
            if (string.IsNullOrWhiteSpace(producto.Nombre))
            {
                return ResultadoOperacion.BadRequest("El nombre del producto es obligatorio.");
            }

            if (producto.Precio < 0 || producto.Stock < 0)
            {
                return ResultadoOperacion.BadRequest("El precio y el stock no pueden ser negativos.");
            }

            if (!await _categoriasRepositorio.ExisteAsync(producto.IdCategoria))
            {
                return ResultadoOperacion.BadRequest("La categoria indicada no existe.");
            }

            if (!await _proveedoresRepositorio.ExisteAsync(producto.IdProveedor))
            {
                return ResultadoOperacion.BadRequest("El proveedor indicado no existe.");
            }

            return ResultadoOperacion.Ok();
        }
    }
}
