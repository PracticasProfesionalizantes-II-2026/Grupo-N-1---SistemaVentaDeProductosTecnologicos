using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface ICarritosLogica : ILogica<Carrito>
    {
        Task<List<Carrito>> ObtenerPorUsuarioAsync(int idUsuario);
        Task<DetalleCarrito?> AgregarProductoAsync(int idCarrito, AgregarProductoCarritoDto dto);
        Task<bool> EliminarProductoAsync(int idCarrito, int idProducto);
        Task<Pedido?> ConfirmarAsync(int idCarrito, ConfirmarCarritoDto dto);
    }

    public class CarritosLogica : Logica<Carrito>, ICarritosLogica
    {
        private readonly ICarritosRepositorio _carritosRepositorio;
        private readonly IDetalleCarritosRepositorio _detalleCarritosRepositorio;
        private readonly IProductosRepositorio _productosRepositorio;
        private readonly IPedidosRepositorio _pedidosRepositorio;
        private readonly IDetallePedidosRepositorio _detallePedidosRepositorio;

        public CarritosLogica(
            ICarritosRepositorio carritosRepositorio,
            IDetalleCarritosRepositorio detalleCarritosRepositorio,
            IProductosRepositorio productosRepositorio,
            IPedidosRepositorio pedidosRepositorio,
            IDetallePedidosRepositorio detallePedidosRepositorio) : base(carritosRepositorio)
        {
            _carritosRepositorio = carritosRepositorio;
            _detalleCarritosRepositorio = detalleCarritosRepositorio;
            _productosRepositorio = productosRepositorio;
            _pedidosRepositorio = pedidosRepositorio;
            _detallePedidosRepositorio = detallePedidosRepositorio;
        }

        public Task<List<Carrito>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return _carritosRepositorio.ObtenerPorUsuarioAsync(idUsuario);
        }

        public async Task<DetalleCarrito?> AgregarProductoAsync(int idCarrito, AgregarProductoCarritoDto dto)
        {
            var carrito = await _carritosRepositorio.ObtenerPorIdAsync(idCarrito);
            var producto = await _productosRepositorio.ObtenerPorIdAsync(dto.IdProducto);

            if (carrito is null || producto is null)
            {
                return null;
            }

            var precio = dto.PrecioUnitario > 0 ? dto.PrecioUnitario : producto.Precio;
            var detalle = new DetalleCarrito
            {
                IdCarrito = idCarrito,
                IdProducto = dto.IdProducto,
                Cantidad = dto.Cantidad,
                PrecioUnitario = precio,
                Subtotal = precio * dto.Cantidad
            };

            await _detalleCarritosRepositorio.CrearAsync(detalle);
            return detalle;
        }

        public async Task<bool> EliminarProductoAsync(int idCarrito, int idProducto)
        {
            var carrito = await _carritosRepositorio.ObtenerPorIdAsync(idCarrito);

            if (carrito is null)
            {
                return false;
            }

            await _detalleCarritosRepositorio.EliminarPorCarritoYProductoAsync(idCarrito, idProducto);
            return true;
        }

        public async Task<Pedido?> ConfirmarAsync(int idCarrito, ConfirmarCarritoDto dto)
        {
            var carrito = await _carritosRepositorio.ObtenerPorIdAsync(idCarrito);

            if (carrito is null || dto.IdDireccion <= 0)
            {
                return null;
            }

            var detallesCarrito = await _detalleCarritosRepositorio.ObtenerPorCarritoAsync(idCarrito);

            if (detallesCarrito.Count == 0)
            {
                return null;
            }

            var pedido = new Pedido
            {
                IdUsuario = carrito.IdUsuario,
                IdDireccion = dto.IdDireccion,
                FechaPedido = DateTime.Now,
                Estado = EstadoPedido.Pendiente
            };

            await _pedidosRepositorio.CrearAsync(pedido);

            foreach (var detalleCarrito in detallesCarrito)
            {
                var detallePedido = new DetallePedido
                {
                    IdPedido = pedido.IdPedido,
                    IdProducto = detalleCarrito.IdProducto,
                    Cantidad = detalleCarrito.Cantidad,
                    PrecioUnitario = detalleCarrito.PrecioUnitario,
                    Subtotal = detalleCarrito.Subtotal
                };

                await _detallePedidosRepositorio.CrearAsync(detallePedido);
            }

            carrito.Estado = EstadoCarrito.Confirmado;
            await _carritosRepositorio.ActualizarAsync(carrito);

            return pedido;
        }
    }
}
