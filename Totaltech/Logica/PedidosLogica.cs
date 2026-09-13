using System.Data;
using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IPedidosLogica
    {
        Task<List<Pedido>> ObtenerTodosAsync();
        Task<Pedido?> ObtenerPorIdAsync(int id);
        Task<List<Pedido>> ObtenerPorUsuarioAsync(int idUsuario);
        Task<List<Pedido>> ObtenerPorEstadoAsync(EstadoPedido estado);
        Task<OperacionDominioResultado> ActualizarEstadoAsync(int id, EstadoPedido estado);
    }

    public class PedidosLogica : IPedidosLogica
    {
        private readonly IPedidosRepositorio _repositorio;
        private readonly TotaltechDbContext _context;

        public PedidosLogica(
            TotaltechDbContext context,
            IPedidosRepositorio repositorio)
        {
            _context = context;
            _repositorio = repositorio;
        }

        public Task<List<Pedido>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<Pedido?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public Task<List<Pedido>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return _repositorio.ObtenerPorUsuarioAsync(idUsuario);
        }

        public Task<List<Pedido>> ObtenerPorEstadoAsync(EstadoPedido estado)
        {
            return _repositorio.ObtenerPorEstadoAsync(estado);
        }

        public async Task<OperacionDominioResultado> ActualizarEstadoAsync(int id, EstadoPedido estado)
        {
            if (!Enum.IsDefined(estado))
            {
                return new(EstadoOperacionDominio.Invalido, "El estado del pedido no es valido.");
            }

            async Task<OperacionDominioResultado> EjecutarAsync(CancellationToken cancellationToken)
            {
                _context.ChangeTracker.Clear();
                var pedido = await _context.Pedidos
                    .SingleOrDefaultAsync(candidato => candidato.IdPedido == id, cancellationToken);
                if (pedido is null)
                {
                    return new(EstadoOperacionDominio.NoEncontrado);
                }

                if (pedido.Estado == estado)
                {
                    return new(EstadoOperacionDominio.Exitoso);
                }

                var transicionValida =
                    (pedido.Estado == EstadoPedido.Pendiente && estado == EstadoPedido.Cancelado) ||
                    (pedido.Estado == EstadoPedido.Pagado && estado == EstadoPedido.Enviado) ||
                    (pedido.Estado == EstadoPedido.Enviado && estado == EstadoPedido.Entregado);

                if (!transicionValida)
                {
                    return new(
                        EstadoOperacionDominio.Conflicto,
                        $"No se puede cambiar un pedido {pedido.Estado} a {estado}.");
                }

                if (estado == EstadoPedido.Cancelado)
                {
                    var detalles = await _context.DetallePedidos
                        .AsNoTracking()
                        .Where(detalle => detalle.IdPedido == id)
                        .ToListAsync(cancellationToken);

                    foreach (var detalle in detalles)
                    {
                        if (_context.Database.IsRelational())
                        {
                            await _context.Productos
                                .Where(producto => producto.IdProducto == detalle.IdProducto)
                                .ExecuteUpdateAsync(
                                    actualizacion => actualizacion.SetProperty(
                                        producto => producto.Stock,
                                        producto => producto.Stock + detalle.Cantidad),
                                    cancellationToken);
                        }
                        else
                        {
                            var producto = await _context.Productos.FindAsync(
                                [detalle.IdProducto],
                                cancellationToken);
                            if (producto is not null)
                            {
                                producto.Stock += detalle.Cantidad;
                            }
                        }
                    }
                }

                pedido.Estado = estado;
                await _context.SaveChangesAsync(cancellationToken);
                return new(EstadoOperacionDominio.Exitoso);
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
                    return await _context.Pedidos
                        .AsNoTracking()
                        .AnyAsync(
                            pedido => pedido.IdPedido == id && pedido.Estado == estado,
                            cancellationToken);
                },
                IsolationLevel.Serializable,
                CancellationToken.None);
        }

    }
}
