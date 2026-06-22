using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface ICarritosLogica : ILogica<Carrito>
    {
        Task<List<Carrito>> ObtenerPorUsuarioAsync(int idUsuario);
        Task<ResultadoOperacion<DetalleCarrito>> AgregarProductoAsync(int idCarrito, AgregarProductoCarritoDto dto);
        Task<ResultadoOperacion> EliminarProductoAsync(int idCarrito, int idProducto);
        Task<ResultadoOperacion<Pedido>> ConfirmarAsync(int idCarrito, ConfirmarCarritoDto dto);
    }

    public class CarritosLogica : Logica<Carrito>, ICarritosLogica
    {
        private readonly TotaltechDbContext _context;
        private readonly ICarritosRepositorio _carritosRepositorio;
        private readonly IDetalleCarritosRepositorio _detalleCarritosRepositorio;
        private readonly IProductosRepositorio _productosRepositorio;
        private readonly IPedidosRepositorio _pedidosRepositorio;
        private readonly IDetallePedidosRepositorio _detallePedidosRepositorio;
        private readonly IUsuariosRepositorio _usuariosRepositorio;
        private readonly IDireccionesRepositorio _direccionesRepositorio;

        public CarritosLogica(
            TotaltechDbContext context,
            ICarritosRepositorio carritosRepositorio,
            IDetalleCarritosRepositorio detalleCarritosRepositorio,
            IProductosRepositorio productosRepositorio,
            IPedidosRepositorio pedidosRepositorio,
            IDetallePedidosRepositorio detallePedidosRepositorio,
            IUsuariosRepositorio usuariosRepositorio,
            IDireccionesRepositorio direccionesRepositorio) : base(carritosRepositorio)
        {
            _context = context;
            _carritosRepositorio = carritosRepositorio;
            _detalleCarritosRepositorio = detalleCarritosRepositorio;
            _productosRepositorio = productosRepositorio;
            _pedidosRepositorio = pedidosRepositorio;
            _detallePedidosRepositorio = detallePedidosRepositorio;
            _usuariosRepositorio = usuariosRepositorio;
            _direccionesRepositorio = direccionesRepositorio;
        }

        public Task<List<Carrito>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return _carritosRepositorio.ObtenerPorUsuarioAsync(idUsuario);
        }

        public override async Task<ResultadoOperacion<Carrito>> CrearValidadoAsync(Carrito carrito)
        {
            var validacion = await ValidarCarritoAsync(carrito);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Carrito>.BadRequest(validacion.Error ?? "El carrito no es valido.");
            }

            if (carrito.FechaCreacion == default)
            {
                carrito.FechaCreacion = DateTime.Now;
            }

            return await base.CrearValidadoAsync(carrito);
        }

        public override async Task<ResultadoOperacion<Carrito>> ActualizarValidadoAsync(int id, Carrito carrito)
        {
            var validacion = await ValidarCarritoAsync(carrito);

            if (!validacion.Exitoso)
            {
                return ResultadoOperacion<Carrito>.BadRequest(validacion.Error ?? "El carrito no es valido.");
            }

            if (carrito.FechaCreacion == default)
            {
                carrito.FechaCreacion = DateTime.Now;
            }

            return await base.ActualizarValidadoAsync(id, carrito);
        }

        public async Task<ResultadoOperacion<DetalleCarrito>> AgregarProductoAsync(int idCarrito, AgregarProductoCarritoDto dto)
        {
            if (dto.Cantidad <= 0)
            {
                return ResultadoOperacion<DetalleCarrito>.BadRequest("La cantidad debe ser mayor a cero.");
            }

            var carrito = await _carritosRepositorio.ObtenerPorIdAsync(idCarrito);
            var producto = await _productosRepositorio.ObtenerPorIdAsync(dto.IdProducto);

            if (carrito is null)
            {
                return ResultadoOperacion<DetalleCarrito>.NotFound("El carrito indicado no existe.");
            }

            if (producto is null)
            {
                return ResultadoOperacion<DetalleCarrito>.NotFound("El producto indicado no existe.");
            }

            if (carrito.Estado != EstadoCarrito.Activo)
            {
                return ResultadoOperacion<DetalleCarrito>.BadRequest("Solo se pueden modificar carritos activos.");
            }

            var detalleExistente = await _detalleCarritosRepositorio.ObtenerPorCarritoYProductoAsync(idCarrito, dto.IdProducto);
            var nuevaCantidad = dto.Cantidad + (detalleExistente?.Cantidad ?? 0);

            if (producto.Stock < nuevaCantidad)
            {
                return ResultadoOperacion<DetalleCarrito>.BadRequest("No hay stock suficiente para agregar ese producto.");
            }

            var precio = dto.PrecioUnitario > 0 ? dto.PrecioUnitario : producto.Precio;

            if (detalleExistente is not null)
            {
                detalleExistente.Cantidad = nuevaCantidad;
                detalleExistente.PrecioUnitario = precio;
                detalleExistente.Subtotal = precio * nuevaCantidad;

                await _detalleCarritosRepositorio.ActualizarAsync(detalleExistente);
                return ResultadoOperacion<DetalleCarrito>.Ok(detalleExistente);
            }

            var detalle = new DetalleCarrito
            {
                IdCarrito = idCarrito,
                IdProducto = dto.IdProducto,
                Cantidad = dto.Cantidad,
                PrecioUnitario = precio,
                Subtotal = precio * dto.Cantidad
            };

            await _detalleCarritosRepositorio.CrearAsync(detalle);
            return ResultadoOperacion<DetalleCarrito>.Ok(detalle);
        }

        public async Task<ResultadoOperacion> EliminarProductoAsync(int idCarrito, int idProducto)
        {
            var carrito = await _carritosRepositorio.ObtenerPorIdAsync(idCarrito);

            if (carrito is null)
            {
                return ResultadoOperacion.NotFound("El carrito indicado no existe.");
            }

            var eliminado = await _detalleCarritosRepositorio.EliminarPorCarritoYProductoAsync(idCarrito, idProducto);
            return eliminado
                ? ResultadoOperacion.Ok()
                : ResultadoOperacion.NotFound("El producto no existe dentro del carrito.");
        }

        public async Task<ResultadoOperacion<Pedido>> ConfirmarAsync(int idCarrito, ConfirmarCarritoDto dto)
        {
            var carrito = await _carritosRepositorio.ObtenerPorIdAsync(idCarrito);

            if (carrito is null)
            {
                return ResultadoOperacion<Pedido>.NotFound("El carrito indicado no existe.");
            }

            if (carrito.Estado != EstadoCarrito.Activo)
            {
                return ResultadoOperacion<Pedido>.BadRequest("Solo se pueden confirmar carritos activos.");
            }

            if (!await _direccionesRepositorio.ExisteAsync(dto.IdDireccion))
            {
                return ResultadoOperacion<Pedido>.BadRequest("La direccion indicada no existe.");
            }

            var detallesCarrito = await _detalleCarritosRepositorio.ObtenerPorCarritoAsync(idCarrito);

            if (detallesCarrito.Count == 0)
            {
                return ResultadoOperacion<Pedido>.BadRequest("El carrito no tiene productos.");
            }

            var productos = new Dictionary<int, Producto>();

            foreach (var detalleCarrito in detallesCarrito)
            {
                var producto = await _productosRepositorio.ObtenerPorIdAsync(detalleCarrito.IdProducto);

                if (producto is null)
                {
                    return ResultadoOperacion<Pedido>.BadRequest("Uno de los productos del carrito ya no existe.");
                }

                if (producto.Stock < detalleCarrito.Cantidad)
                {
                    return ResultadoOperacion<Pedido>.BadRequest($"No hay stock suficiente para el producto {producto.Nombre}.");
                }

                productos[producto.IdProducto] = producto;
            }

            // El pedido, sus detalles, el stock y el estado del carrito deben cambiar juntos.
            await using var transaction = await _context.Database.BeginTransactionAsync();

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
                var producto = productos[detalleCarrito.IdProducto];

                var detallePedido = new DetallePedido
                {
                    IdPedido = pedido.IdPedido,
                    IdProducto = detalleCarrito.IdProducto,
                    Cantidad = detalleCarrito.Cantidad,
                    PrecioUnitario = detalleCarrito.PrecioUnitario,
                    Subtotal = detalleCarrito.Subtotal
                };

                producto.Stock -= detalleCarrito.Cantidad;
                await _detallePedidosRepositorio.CrearAsync(detallePedido);
                await _productosRepositorio.ActualizarAsync(producto);
            }

            carrito.Estado = EstadoCarrito.Confirmado;
            await _carritosRepositorio.ActualizarAsync(carrito);

            await transaction.CommitAsync();
            return ResultadoOperacion<Pedido>.Ok(pedido);
        }

        private async Task<ResultadoOperacion> ValidarCarritoAsync(Carrito carrito)
        {
            if (!await _usuariosRepositorio.ExisteAsync(carrito.IdUsuario))
            {
                return ResultadoOperacion.BadRequest("El usuario indicado no existe.");
            }

            return ResultadoOperacion.Ok();
        }
    }
}
