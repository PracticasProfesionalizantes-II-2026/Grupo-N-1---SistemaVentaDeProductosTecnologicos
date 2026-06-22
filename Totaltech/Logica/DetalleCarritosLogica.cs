using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IDetalleCarritosLogica
    {
        Task<List<DetalleCarrito>> ObtenerTodosAsync();
        Task<DetalleCarrito?> ObtenerPorIdAsync(int id);
        Task<List<DetalleCarrito>> ObtenerPorCarritoAsync(int idCarrito);
        Task<string?> CrearAsync(DetalleCarrito detalle);
        Task<string?> ActualizarAsync(DetalleCarrito detalle);
        Task<bool> EliminarAsync(int id);
    }

    public class DetalleCarritosLogica : IDetalleCarritosLogica
    {
        private readonly IDetalleCarritosRepositorio _repositorio;
        private readonly ICarritosRepositorio _carritosRepositorio;
        private readonly IProductosRepositorio _productosRepositorio;

        public DetalleCarritosLogica(
            IDetalleCarritosRepositorio repositorio,
            ICarritosRepositorio carritosRepositorio,
            IProductosRepositorio productosRepositorio)
        {
            _repositorio = repositorio;
            _carritosRepositorio = carritosRepositorio;
            _productosRepositorio = productosRepositorio;
        }

        public Task<List<DetalleCarrito>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<DetalleCarrito?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public Task<List<DetalleCarrito>> ObtenerPorCarritoAsync(int idCarrito)
        {
            return _repositorio.ObtenerPorCarritoAsync(idCarrito);
        }

        public async Task<string?> CrearAsync(DetalleCarrito detalle)
        {
            var error = await ValidarDetalleAsync(detalle);
            if (error is not null)
            {
                return error;
            }

            var existente = await _repositorio.ObtenerPorCarritoYProductoAsync(detalle.IdCarrito, detalle.IdProducto);
            if (existente is not null)
            {
                return "El producto ya existe en el carrito.";
            }

            detalle.Subtotal = detalle.PrecioUnitario * detalle.Cantidad;
            await _repositorio.CrearAsync(detalle);
            return null;
        }

        public async Task<string?> ActualizarAsync(DetalleCarrito detalle)
        {
            var error = await ValidarDetalleAsync(detalle);
            if (error is not null)
            {
                return error;
            }

            detalle.Subtotal = detalle.PrecioUnitario * detalle.Cantidad;
            await _repositorio.ActualizarAsync(detalle);
            return null;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var detalle = await _repositorio.ObtenerPorIdAsync(id);
            if (detalle is null)
            {
                return false;
            }

            await _repositorio.EliminarAsync(detalle);
            return true;
        }

        private async Task<string?> ValidarDetalleAsync(DetalleCarrito detalle)
        {
            if (detalle.Cantidad <= 0 || detalle.PrecioUnitario < 0)
            {
                return "La cantidad debe ser mayor a cero y el precio no puede ser negativo.";
            }

            if (!await _carritosRepositorio.ExisteAsync(detalle.IdCarrito))
            {
                return "El carrito indicado no existe.";
            }

            if (!await _productosRepositorio.ExisteAsync(detalle.IdProducto))
            {
                return "El producto indicado no existe.";
            }

            return null;
        }
    }
}
