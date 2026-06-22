using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IDetallePedidosLogica
    {
        Task<List<DetallePedido>> ObtenerTodosAsync();
        Task<DetallePedido?> ObtenerPorIdAsync(int id);
        Task<string?> CrearAsync(DetallePedido detalle);
        Task<string?> ActualizarAsync(DetallePedido detalle);
        Task<bool> EliminarAsync(int id);
    }

    public class DetallePedidosLogica : IDetallePedidosLogica
    {
        private readonly IDetallePedidosRepositorio _repositorio;
        private readonly IPedidosRepositorio _pedidosRepositorio;
        private readonly IProductosRepositorio _productosRepositorio;

        public DetallePedidosLogica(
            IDetallePedidosRepositorio repositorio,
            IPedidosRepositorio pedidosRepositorio,
            IProductosRepositorio productosRepositorio)
        {
            _repositorio = repositorio;
            _pedidosRepositorio = pedidosRepositorio;
            _productosRepositorio = productosRepositorio;
        }

        public Task<List<DetallePedido>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<DetallePedido?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<string?> CrearAsync(DetallePedido detalle)
        {
            var error = await ValidarDetalleAsync(detalle);
            if (error is not null)
            {
                return error;
            }

            detalle.Subtotal = detalle.PrecioUnitario * detalle.Cantidad;
            await _repositorio.CrearAsync(detalle);
            return null;
        }

        public async Task<string?> ActualizarAsync(DetallePedido detalle)
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

        private async Task<string?> ValidarDetalleAsync(DetallePedido detalle)
        {
            if (detalle.Cantidad <= 0 || detalle.PrecioUnitario < 0)
            {
                return "La cantidad debe ser mayor a cero y el precio no puede ser negativo.";
            }

            if (!await _pedidosRepositorio.ExisteAsync(detalle.IdPedido))
            {
                return "El pedido indicado no existe.";
            }

            if (!await _productosRepositorio.ExisteAsync(detalle.IdProducto))
            {
                return "El producto indicado no existe.";
            }

            return null;
        }
    }
}
