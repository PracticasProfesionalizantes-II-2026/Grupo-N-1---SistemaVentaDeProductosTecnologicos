using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface ICarritosLogica
    {
        Task<List<Carrito>> ObtenerTodosAsync();
        Task<Carrito?> ObtenerPorIdAsync(int id);
        Task<List<Carrito>> ObtenerPorUsuarioAsync(int idUsuario);
        Task<string?> CrearAsync(Carrito carrito);
        Task<string?> ActualizarAsync(Carrito carrito);
        Task<bool> EliminarAsync(int id);
        Task<(DetalleCarrito? Detalle, string? Error)> AgregarProductoAsync(int idCarrito, AgregarProductoCarritoDto dto);
        Task<string?> EliminarProductoAsync(int idCarrito, int idProducto);
        Task<(Pedido? Pedido, string? Error)> ConfirmarAsync(int idCarrito, ConfirmarCarritoDto dto);
    }

    public class CarritosLogica : ICarritosLogica
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
            IDireccionesRepositorio direccionesRepositorio)
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

        public Task<List<Carrito>> ObtenerTodosAsync()
        {
            return _carritosRepositorio.ObtenerTodosAsync();
        }

        public Task<Carrito?> ObtenerPorIdAsync(int id)
        {
            return _carritosRepositorio.ObtenerPorIdAsync(id);
        }

        public Task<List<Carrito>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return _carritosRepositorio.ObtenerPorUsuarioAsync(idUsuario);
        }

        public async Task<string?> CrearAsync(Carrito carrito)
        {
            var error = await ValidarCarritoAsync(carrito);
            if (error is not null)
            {
                return error;
            }

            if (carrito.FechaCreacion == default)
            {
                carrito.FechaCreacion = DateTime.Now;
            }

            await _carritosRepositorio.CrearAsync(carrito);
            return null;
        }

        public async Task<string?> ActualizarAsync(Carrito carrito)
        {
            var error = await ValidarCarritoAsync(carrito);
            if (error is not null)
            {
                return error;
            }

            if (carrito.FechaCreacion == default)
            {
                carrito.FechaCreacion = DateTime.Now;
            }

            await _carritosRepositorio.ActualizarAsync(carrito);
            return null;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var carrito = await _carritosRepositorio.ObtenerPorIdAsync(id);
            if (carrito is null)
            {
                return false;
            }

            await _carritosRepositorio.EliminarAsync(carrito);
            return true;
        }

        public async Task<(DetalleCarrito? Detalle, string? Error)> AgregarProductoAsync(int idCarrito, AgregarProductoCarritoDto dto)
        {
            if (dto.Cantidad <= 0)
            {
                return (null, "La cantidad debe ser mayor a cero.");
            }

            var carrito = await _carritosRepositorio.ObtenerPorIdAsync(idCarrito);
            if (carrito is null)
            {
                return (null, "El carrito indicado no existe.");
            }

            if (carrito.Estado != EstadoCarrito.Activo)
            {
                return (null, "Solo se pueden modificar carritos activos.");
            }

            var producto = await _productosRepositorio.ObtenerPorIdAsync(dto.IdProducto);
            if (producto is null)
            {
                return (null, "El producto indicado no existe.");
            }

            var detalleExistente = await _detalleCarritosRepositorio.ObtenerPorCarritoYProductoAsync(idCarrito, dto.IdProducto);
            var nuevaCantidad = dto.Cantidad + (detalleExistente?.Cantidad ?? 0);

            if (producto.Stock < nuevaCantidad)
            {
                return (null, "No hay stock suficiente para agregar ese producto.");
            }

            var precio = dto.PrecioUnitario > 0 ? dto.PrecioUnitario : producto.Precio;

            if (detalleExistente is not null)
            {
                detalleExistente.Cantidad = nuevaCantidad;
                detalleExistente.PrecioUnitario = precio;
                detalleExistente.Subtotal = precio * nuevaCantidad;

                await _detalleCarritosRepositorio.ActualizarAsync(detalleExistente);
                return (detalleExistente, null);
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
            return (detalle, null);
        }

        public async Task<string?> EliminarProductoAsync(int idCarrito, int idProducto)
        {
            var carrito = await _carritosRepositorio.ObtenerPorIdAsync(idCarrito);
            if (carrito is null)
            {
                return "El carrito indicado no existe.";
            }

            if (carrito.Estado != EstadoCarrito.Activo)
            {
                return "Solo se pueden modificar carritos activos.";
            }

            var eliminado = await _detalleCarritosRepositorio.EliminarPorCarritoYProductoAsync(idCarrito, idProducto);
            return eliminado ? null : "El producto no existe dentro del carrito.";
        }

        public async Task<(Pedido? Pedido, string? Error)> ConfirmarAsync(int idCarrito, ConfirmarCarritoDto dto)
        {
            var carrito = await _carritosRepositorio.ObtenerPorIdAsync(idCarrito);
            if (carrito is null)
            {
                return (null, "El carrito indicado no existe.");
            }

            if (carrito.Estado != EstadoCarrito.Activo)
            {
                return (null, "Solo se pueden confirmar carritos activos.");
            }

            var direccion = await _direccionesRepositorio.ObtenerPorIdAsync(dto.IdDireccion);
            if (direccion is null)
            {
                return (null, "La direccion indicada no existe.");
            }

            if (direccion.IdUsuario != carrito.IdUsuario)
            {
                return (null, "La direccion indicada no pertenece al usuario del carrito.");
            }

            var detallesCarrito = await _detalleCarritosRepositorio.ObtenerPorCarritoAsync(idCarrito);
            if (detallesCarrito.Count == 0)
            {
                return (null, "El carrito no tiene productos.");
            }

            var productos = new Dictionary<int, Producto>();
            foreach (var detalleCarrito in detallesCarrito)
            {
                var producto = await _productosRepositorio.ObtenerPorIdAsync(detalleCarrito.IdProducto);
                if (producto is null)
                {
                    return (null, "Uno de los productos del carrito ya no existe.");
                }

                if (producto.Stock < detalleCarrito.Cantidad)
                {
                    return (null, $"No hay stock suficiente para el producto {producto.Nombre}.");
                }

                productos[producto.IdProducto] = producto;
            }

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
            return (pedido, null);
        }

        private async Task<string?> ValidarCarritoAsync(Carrito carrito)
        {
            if (!Enum.IsDefined(carrito.Estado))
            {
                return "El estado del carrito no es valido.";
            }

            if (!await _usuariosRepositorio.ExisteAsync(carrito.IdUsuario))
            {
                return "El usuario indicado no existe.";
            }

            return null;
        }
    }
}
