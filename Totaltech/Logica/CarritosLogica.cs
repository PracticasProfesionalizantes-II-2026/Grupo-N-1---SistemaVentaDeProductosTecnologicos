using System.Data;
using Microsoft.EntityFrameworkCore;
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
        Task<CarritoResumenResponse> ObtenerResumenActivoAsync(int idUsuario, CancellationToken cancellationToken = default);
        Task<(CarritoResumenResponse? Resumen, string? Error, bool Conflicto)> AgregarProductoActivoAsync(
            int idUsuario, AgregarProductoCarritoDto dto, CancellationToken cancellationToken = default);
        Task<(CarritoResumenResponse? Resumen, string? Error, bool Conflicto)> ActualizarCantidadActivaAsync(
            int idUsuario, int idProducto, int cantidad, CancellationToken cancellationToken = default);
        Task<(CarritoResumenResponse? Resumen, string? Error)> EliminarProductoActivoAsync(
            int idUsuario, int idProducto, CancellationToken cancellationToken = default);
        Task<ConfirmarCarritoResultado> ConfirmarAsync(int idCarrito, ConfirmarCarritoDto dto);
    }

    public class CarritosLogica : ICarritosLogica
    {
        private readonly TotaltechDbContext _context;
        private readonly ICarritosRepositorio _carritosRepositorio;
        private readonly IProductosRepositorio _productosRepositorio;
        private readonly IUsuariosRepositorio _usuariosRepositorio;

        public CarritosLogica(
            TotaltechDbContext context,
            ICarritosRepositorio carritosRepositorio,
            IProductosRepositorio productosRepositorio,
            IUsuariosRepositorio usuariosRepositorio)
        {
            _context = context;
            _carritosRepositorio = carritosRepositorio;
            _productosRepositorio = productosRepositorio;
            _usuariosRepositorio = usuariosRepositorio;
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

            var cantidadEsperada = 0;
            async Task<(DetalleCarrito? Detalle, string? Error)> EjecutarAsync(
                CancellationToken cancellationToken)
            {
                _context.ChangeTracker.Clear();
                var carrito = await _context.Carritos
                    .SingleOrDefaultAsync(candidato => candidato.IdCarrito == idCarrito, cancellationToken);
                if (carrito is null)
                {
                    return (null, "El carrito indicado no existe.");
                }

                if (carrito.Estado != EstadoCarrito.Activo)
                {
                    return (null, "Solo se pueden modificar carritos activos.");
                }

                var producto = await _context.Productos
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        candidato => candidato.IdProducto == dto.IdProducto,
                        cancellationToken);
                if (producto is null)
                {
                    return (null, "El producto indicado no existe.");
                }

                var detalleExistente = await _context.DetalleCarritos
                    .SingleOrDefaultAsync(
                        detalle => detalle.IdCarrito == idCarrito && detalle.IdProducto == dto.IdProducto,
                        cancellationToken);
                cantidadEsperada = checked(dto.Cantidad + (detalleExistente?.Cantidad ?? 0));

                if (producto.Stock < cantidadEsperada)
                {
                    return (null, "No hay stock suficiente para agregar ese producto.");
                }

                if (detalleExistente is not null)
                {
                    detalleExistente.Cantidad = cantidadEsperada;
                    detalleExistente.PrecioUnitario = producto.Precio;
                    detalleExistente.Subtotal = producto.Precio * cantidadEsperada;
                    await _context.SaveChangesAsync(cancellationToken);
                    return (detalleExistente, null);
                }

                var detalle = new DetalleCarrito
                {
                    IdCarrito = idCarrito,
                    IdProducto = dto.IdProducto,
                    Cantidad = cantidadEsperada,
                    PrecioUnitario = producto.Precio,
                    Subtotal = producto.Precio * cantidadEsperada
                };

                _context.DetalleCarritos.Add(detalle);
                await _context.SaveChangesAsync(cancellationToken);
                return (detalle, null);
            }

            if (!_context.Database.IsRelational())
            {
                return await EjecutarAsync(CancellationToken.None);
            }

            var estrategia = _context.Database.CreateExecutionStrategy();
            return await Microsoft.EntityFrameworkCore.Storage.RelationalExecutionStrategyExtensions.ExecuteInTransactionAsync(
                estrategia,
                EjecutarAsync,
                async cancellationToken =>
                {
                    _context.ChangeTracker.Clear();
                    return await _context.DetalleCarritos.AsNoTracking().AnyAsync(
                        detalle =>
                            detalle.IdCarrito == idCarrito &&
                            detalle.IdProducto == dto.IdProducto &&
                            detalle.Cantidad == cantidadEsperada,
                        cancellationToken);
                },
                IsolationLevel.Serializable,
                CancellationToken.None);
        }

        public async Task<string?> EliminarProductoAsync(int idCarrito, int idProducto)
        {
            async Task<string?> EjecutarAsync(CancellationToken cancellationToken)
            {
                _context.ChangeTracker.Clear();
                var carrito = await _context.Carritos
                    .SingleOrDefaultAsync(candidato => candidato.IdCarrito == idCarrito, cancellationToken);
                if (carrito is null)
                {
                    return "El carrito indicado no existe.";
                }

                if (carrito.Estado != EstadoCarrito.Activo)
                {
                    return "Solo se pueden modificar carritos activos.";
                }

                var detalle = await _context.DetalleCarritos.SingleOrDefaultAsync(
                    candidato => candidato.IdCarrito == idCarrito && candidato.IdProducto == idProducto,
                    cancellationToken);
                if (detalle is null)
                {
                    return "El producto no existe dentro del carrito.";
                }

                _context.DetalleCarritos.Remove(detalle);
                await _context.SaveChangesAsync(cancellationToken);
                return null;
            }

            if (!_context.Database.IsRelational())
            {
                return await EjecutarAsync(CancellationToken.None);
            }

            var estrategia = _context.Database.CreateExecutionStrategy();
            return await Microsoft.EntityFrameworkCore.Storage.RelationalExecutionStrategyExtensions.ExecuteInTransactionAsync(
                estrategia,
                EjecutarAsync,
                async cancellationToken =>
                {
                    _context.ChangeTracker.Clear();
                    return !await _context.DetalleCarritos.AsNoTracking().AnyAsync(
                        detalle => detalle.IdCarrito == idCarrito && detalle.IdProducto == idProducto,
                        cancellationToken);
                },
                IsolationLevel.Serializable,
                CancellationToken.None);
        }

        public async Task<CarritoResumenResponse> ObtenerResumenActivoAsync(
            int idUsuario, CancellationToken cancellationToken = default)
        {
            var carrito = await _context.Carritos
                .AsNoTracking()
                .Where(item => item.IdUsuario == idUsuario && item.Estado == EstadoCarrito.Activo)
                .OrderByDescending(item => item.IdCarrito)
                .FirstOrDefaultAsync(cancellationToken);
            return carrito is null
                ? new CarritoResumenResponse()
                : await CrearResumenAsync(carrito.IdCarrito, cancellationToken);
        }

        public Task<(CarritoResumenResponse? Resumen, string? Error, bool Conflicto)> AgregarProductoActivoAsync(
            int idUsuario, AgregarProductoCarritoDto dto, CancellationToken cancellationToken = default) =>
            ModificarCarritoActivoAsync(idUsuario, dto.IdProducto, dto.Cantidad, true, cancellationToken);

        public Task<(CarritoResumenResponse? Resumen, string? Error, bool Conflicto)> ActualizarCantidadActivaAsync(
            int idUsuario, int idProducto, int cantidad, CancellationToken cancellationToken = default) =>
            ModificarCarritoActivoAsync(idUsuario, idProducto, cantidad, false, cancellationToken);

        public async Task<(CarritoResumenResponse? Resumen, string? Error)> EliminarProductoActivoAsync(
            int idUsuario, int idProducto, CancellationToken cancellationToken = default)
        {
            if (!_context.Database.IsRelational())
                return await EliminarProductoActivoEnTransaccionAsync(idUsuario, idProducto, cancellationToken);

            var estrategia = _context.Database.CreateExecutionStrategy();
            return await estrategia.ExecuteAsync(() =>
                EliminarProductoActivoEnTransaccionAsync(idUsuario, idProducto, cancellationToken));
        }

        private async Task<(CarritoResumenResponse? Resumen, string? Error)> EliminarProductoActivoEnTransaccionAsync(
            int idUsuario, int idProducto, CancellationToken cancellationToken = default)
        {
            if (idProducto <= 0)
                return (null, "El producto indicado no es válido.");

            await using var transaccion = _context.Database.IsRelational()
                ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                : null;
            try
            {
                _context.ChangeTracker.Clear();
                var carrito = await ObtenerCarritoActivoAsync(idUsuario, false, cancellationToken);
                if (carrito is null)
                    return (null, "El carrito no contiene ese producto.");

                var detalle = await _context.DetalleCarritos.SingleOrDefaultAsync(item =>
                    item.IdCarrito == carrito.IdCarrito && item.IdProducto == idProducto, cancellationToken);
                if (detalle is null)
                    return (null, "El carrito no contiene ese producto.");

                _context.DetalleCarritos.Remove(detalle);
                await _context.SaveChangesAsync(cancellationToken);
                if (transaccion is not null) await transaccion.CommitAsync(cancellationToken);
                return (await CrearResumenAsync(carrito.IdCarrito, cancellationToken), null);
            }
            catch (DbUpdateException)
            {
                if (transaccion is not null) await transaccion.RollbackAsync(cancellationToken);
                return (null, "No se pudo actualizar el carrito. Intentá nuevamente.");
            }
        }

        private async Task<(CarritoResumenResponse? Resumen, string? Error, bool Conflicto)> ModificarCarritoActivoAsync(
            int idUsuario, int idProducto, int cantidad, bool incrementar, CancellationToken cancellationToken)
        {
            if (!_context.Database.IsRelational())
                return await ModificarCarritoActivoEnTransaccionAsync(idUsuario, idProducto, cantidad, incrementar, cancellationToken);

            var estrategia = _context.Database.CreateExecutionStrategy();
            return await estrategia.ExecuteAsync(() =>
                ModificarCarritoActivoEnTransaccionAsync(idUsuario, idProducto, cantidad, incrementar, cancellationToken));
        }

        private async Task<(CarritoResumenResponse? Resumen, string? Error, bool Conflicto)> ModificarCarritoActivoEnTransaccionAsync(
            int idUsuario, int idProducto, int cantidad, bool incrementar, CancellationToken cancellationToken)
        {
            if (idProducto <= 0 || cantidad <= 0)
                return (null, "La cantidad debe ser mayor a cero y el producto válido.", false);

            await using var transaccion = _context.Database.IsRelational()
                ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                : null;
            try
            {
                _context.ChangeTracker.Clear();
                var carrito = await ObtenerCarritoActivoAsync(idUsuario, incrementar, cancellationToken);
                if (carrito is null)
                    return (null, "El carrito no contiene ese producto.", false);
                var producto = await _context.Productos.SingleOrDefaultAsync(item => item.IdProducto == idProducto, cancellationToken);
                if (producto is null)
                    return (null, "El producto indicado no existe.", false);

                var detalle = await _context.DetalleCarritos.SingleOrDefaultAsync(item =>
                    item.IdCarrito == carrito.IdCarrito && item.IdProducto == idProducto, cancellationToken);
                var cantidadFinal = incrementar ? checked((detalle?.Cantidad ?? 0) + cantidad) : cantidad;
                if (cantidadFinal > producto.Stock)
                    return (null, "No hay stock suficiente para esa cantidad.", true);

                if (detalle is null)
                {
                    detalle = new DetalleCarrito { IdCarrito = carrito.IdCarrito, IdProducto = idProducto };
                    _context.DetalleCarritos.Add(detalle);
                }
                detalle.Cantidad = cantidadFinal;
                detalle.PrecioUnitario = producto.Precio;
                detalle.Subtotal = producto.Precio * cantidadFinal;
                await _context.SaveChangesAsync(cancellationToken);
                if (transaccion is not null) await transaccion.CommitAsync(cancellationToken);
                return (await CrearResumenAsync(carrito.IdCarrito, cancellationToken), null, false);
            }
            catch (DbUpdateException)
            {
                if (transaccion is not null) await transaccion.RollbackAsync(cancellationToken);
                return (null, "El carrito fue actualizado por otra operación. Intentá nuevamente.", true);
            }
        }

        private async Task<Carrito> ObtenerCarritoActivoAsync(
            int idUsuario, bool crearSiNoExiste, CancellationToken cancellationToken)
        {
            var carrito = await _context.Carritos
                .Where(item => item.IdUsuario == idUsuario && item.Estado == EstadoCarrito.Activo)
                .OrderByDescending(item => item.IdCarrito)
                .FirstOrDefaultAsync(cancellationToken);
            if (carrito is not null || !crearSiNoExiste)
                return carrito!;

            carrito = new Carrito { IdUsuario = idUsuario, FechaCreacion = DateTime.UtcNow, Estado = EstadoCarrito.Activo };
            _context.Carritos.Add(carrito);
            await _context.SaveChangesAsync(cancellationToken);
            return carrito;
        }

        private async Task<CarritoResumenResponse> CrearResumenAsync(int idCarrito, CancellationToken cancellationToken)
        {
            var items = await _context.DetalleCarritos
                .AsNoTracking()
                .Where(detalle => detalle.IdCarrito == idCarrito)
                .Join(_context.Productos.AsNoTracking(), detalle => detalle.IdProducto, producto => producto.IdProducto,
                    (detalle, producto) => new LineaCarritoResponse
                    {
                        IdProducto = producto.IdProducto,
                        Nombre = producto.Nombre,
                        Descripcion = producto.Descripcion,
                        PrecioUnitario = producto.Precio,
                        Cantidad = detalle.Cantidad,
                        Subtotal = producto.Precio * detalle.Cantidad
                    })
                .OrderBy(item => item.Nombre)
                .ThenBy(item => item.IdProducto)
                .ToListAsync(cancellationToken);
            return new CarritoResumenResponse
            {
                IdCarrito = idCarrito, Items = items,
                CantidadTotal = items.Sum(item => item.Cantidad),
                Total = items.Sum(item => item.Subtotal)
            };
        }

        public async Task<ConfirmarCarritoResultado> ConfirmarAsync(int idCarrito, ConfirmarCarritoDto dto)
        {
            if (dto.IdDireccion <= 0)
            {
                return new(EstadoConfirmacionCarrito.Invalido, Error: "La direccion indicada no es valida.");
            }

            async Task<ConfirmarCarritoResultado> EjecutarIntentoAsync(CancellationToken cancellationToken)
            {
                _context.ChangeTracker.Clear();

                var pedidoExistente = await _context.Pedidos
                    .AsNoTracking()
                    .SingleOrDefaultAsync(pedido => pedido.IdCarrito == idCarrito, cancellationToken);
                if (pedidoExistente is not null)
                {
                    return pedidoExistente.IdDireccion == dto.IdDireccion
                        ? new(EstadoConfirmacionCarrito.Repetido, pedidoExistente)
                        : new(
                            EstadoConfirmacionCarrito.Conflicto,
                            Error: "El carrito ya fue confirmado con una direccion diferente.");
                }

                var carrito = await _context.Carritos
                    .SingleOrDefaultAsync(candidato => candidato.IdCarrito == idCarrito, cancellationToken);
                if (carrito is null)
                {
                    return new(EstadoConfirmacionCarrito.NoEncontrado, Error: "El carrito indicado no existe.");
                }

                if (carrito.Estado != EstadoCarrito.Activo)
                {
                    return new(
                        EstadoConfirmacionCarrito.Conflicto,
                        Error: "Solo se pueden confirmar carritos activos.");
                }

                var direccion = await _context.Direcciones
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        candidata => candidata.IdDireccion == dto.IdDireccion,
                        cancellationToken);
                if (direccion is null)
                {
                    return new(
                        EstadoConfirmacionCarrito.NoEncontrado,
                        Error: "La direccion indicada no existe.");
                }

                if (direccion.IdUsuario != carrito.IdUsuario)
                {
                    return new(
                        EstadoConfirmacionCarrito.Conflicto,
                        Error: "La direccion indicada no pertenece al usuario del carrito.");
                }

                var detallesCarrito = await _context.DetalleCarritos
                    .AsNoTracking()
                    .Where(detalle => detalle.IdCarrito == idCarrito)
                    .ToListAsync(cancellationToken);
                if (detallesCarrito.Count == 0)
                {
                    return new(EstadoConfirmacionCarrito.Invalido, Error: "El carrito no tiene productos.");
                }

                var idsProductos = detallesCarrito.Select(detalle => detalle.IdProducto).ToArray();
                var productos = await _context.Productos
                    .AsNoTracking()
                    .Where(producto => idsProductos.Contains(producto.IdProducto))
                    .ToDictionaryAsync(producto => producto.IdProducto, cancellationToken);

                foreach (var detalleCarrito in detallesCarrito)
                {
                    if (!productos.TryGetValue(detalleCarrito.IdProducto, out var producto))
                    {
                        return new(
                            EstadoConfirmacionCarrito.NoEncontrado,
                            Error: "Uno de los productos del carrito ya no existe.");
                    }

                    if (producto.Stock < detalleCarrito.Cantidad)
                    {
                        return new(
                            EstadoConfirmacionCarrito.Conflicto,
                            Error: $"No hay stock suficiente para el producto {producto.Nombre}.");
                    }
                }

                if (_context.Database.IsRelational())
                {
                    var carritoTomado = await _context.Carritos
                        .Where(candidato =>
                            candidato.IdCarrito == idCarrito &&
                            candidato.Estado == EstadoCarrito.Activo)
                        .ExecuteUpdateAsync(
                            actualizacion => actualizacion.SetProperty(
                                candidato => candidato.Estado,
                                EstadoCarrito.Confirmado),
                            cancellationToken);

                    if (carritoTomado != 1)
                    {
                        throw new ConfirmacionCarritoException(
                            EstadoConfirmacionCarrito.Conflicto,
                            "El carrito fue modificado mientras se confirmaba.");
                    }
                }
                else
                {
                    carrito.Estado = EstadoCarrito.Confirmado;
                }

                decimal total;
                try
                {
                    total = detallesCarrito.Sum(detalle =>
                        checked(productos[detalle.IdProducto].Precio * detalle.Cantidad));
                }
                catch (OverflowException)
                {
                    throw new ConfirmacionCarritoException(
                        EstadoConfirmacionCarrito.Invalido,
                        "El total del carrito excede el importe permitido.");
                }

                var pedido = new Pedido
                {
                    IdUsuario = carrito.IdUsuario,
                    IdCarrito = carrito.IdCarrito,
                    IdDireccion = dto.IdDireccion,
                    FechaPedido = DateTime.UtcNow,
                    Estado = EstadoPedido.Pendiente,
                    Total = total,
                    DireccionCalle = direccion.Calle,
                    DireccionNumero = direccion.Numero,
                    DireccionCiudad = direccion.Ciudad,
                    DireccionProvincia = direccion.Provincia,
                    DireccionCodigoPostal = direccion.CodigoPostal,
                    DireccionPais = direccion.Pais
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync(cancellationToken);

                foreach (var detalleCarrito in detallesCarrito)
                {
                    var producto = productos[detalleCarrito.IdProducto];
                    if (!await _productosRepositorio.DescontarStockAsync(
                            producto.IdProducto,
                            detalleCarrito.Cantidad))
                    {
                        throw new ConfirmacionCarritoException(
                            EstadoConfirmacionCarrito.Conflicto,
                            $"No hay stock suficiente para el producto {producto.Nombre}.");
                    }

                    _context.DetallePedidos.Add(new DetallePedido
                    {
                        IdPedido = pedido.IdPedido,
                        IdProducto = detalleCarrito.IdProducto,
                        Cantidad = detalleCarrito.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = producto.Precio * detalleCarrito.Cantidad
                    });
                }

                await _context.SaveChangesAsync(cancellationToken);
                return new(EstadoConfirmacionCarrito.Creado, pedido);
            }

            try
            {
                if (!_context.Database.IsRelational())
                {
                    return await EjecutarIntentoAsync(CancellationToken.None);
                }

                var estrategia = _context.Database.CreateExecutionStrategy();
                return await Microsoft.EntityFrameworkCore.Storage.RelationalExecutionStrategyExtensions.ExecuteInTransactionAsync(
                    estrategia,
                    EjecutarIntentoAsync,
                    async cancellationToken =>
                    {
                        _context.ChangeTracker.Clear();
                        return await _context.Pedidos
                            .AsNoTracking()
                            .AnyAsync(pedido => pedido.IdCarrito == idCarrito, cancellationToken);
                    },
                    IsolationLevel.Serializable,
                    CancellationToken.None);
            }
            catch (ConfirmacionCarritoException ex)
            {
                _context.ChangeTracker.Clear();
                return new(ex.Estado, Error: ex.Message);
            }
            catch (DbUpdateException)
            {
                _context.ChangeTracker.Clear();
                var pedidoExistente = await _context.Pedidos
                    .AsNoTracking()
                    .SingleOrDefaultAsync(pedido => pedido.IdCarrito == idCarrito);

                if (pedidoExistente is not null)
                {
                    return pedidoExistente.IdDireccion == dto.IdDireccion
                        ? new(EstadoConfirmacionCarrito.Repetido, pedidoExistente)
                        : new(
                            EstadoConfirmacionCarrito.Conflicto,
                            Error: "El carrito ya fue confirmado con una direccion diferente.");
                }

                throw;
            }
        }

        private sealed class ConfirmacionCarritoException : Exception
        {
            public ConfirmacionCarritoException(EstadoConfirmacionCarrito estado, string message)
                : base(message)
            {
                Estado = estado;
            }

            public EstadoConfirmacionCarrito Estado { get; }
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
