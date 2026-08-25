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
        Task<(bool Eliminado, string? Error)> EliminarAsync(int id);
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

            var existente = await _repositorio.ObtenerPorCarritoYProductoAsync(detalle.IdCarrito, detalle.IdProducto);
            if (existente is not null && existente.IdDetalleCarrito != detalle.IdDetalleCarrito)
            {
                return "El producto ya existe en el carrito.";
            }

            detalle.Subtotal = detalle.PrecioUnitario * detalle.Cantidad;
            await _repositorio.ActualizarAsync(detalle);
            return null;
        }

        public async Task<(bool Eliminado, string? Error)> EliminarAsync(int id)
        {
            var detalle = await _repositorio.ObtenerPorIdAsync(id);
            if (detalle is null)
            {
                return (false, null);
            }

            var carrito = await _carritosRepositorio.ObtenerPorIdAsync(detalle.IdCarrito);
            if (carrito is null)
            {
                return (false, "El carrito indicado no existe.");
            }

            if (carrito.Estado != EstadoCarrito.Activo)
            {
                return (false, "Solo se pueden modificar carritos activos.");
            }

            await _repositorio.EliminarAsync(detalle);
            return (true, null);
        }

        private async Task<string?> ValidarDetalleAsync(DetalleCarrito detalle)
        {
            if (detalle.Cantidad <= 0 || detalle.PrecioUnitario < 0)
            {
                return "La cantidad debe ser mayor a cero y el precio no puede ser negativo.";
            }

            var carrito = await _carritosRepositorio.ObtenerPorIdAsync(detalle.IdCarrito);
            if (carrito is null)
            {
                return "El carrito indicado no existe.";
            }

            if (carrito.Estado != EstadoCarrito.Activo)
            {
                return "Solo se pueden modificar carritos activos.";
            }

            var producto = await _productosRepositorio.ObtenerPorIdAsync(detalle.IdProducto);
            if (producto is null)
            {
                return "El producto indicado no existe.";
            }

            if (producto.Stock < detalle.Cantidad)
            {
                return "No hay stock suficiente para agregar ese producto.";
            }

            return null;
        }
    }
}
