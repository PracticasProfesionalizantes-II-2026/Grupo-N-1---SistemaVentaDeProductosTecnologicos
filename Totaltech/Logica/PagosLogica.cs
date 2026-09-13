using System.Data;
using Microsoft.EntityFrameworkCore;
using Totaltech.Datos;
using Totaltech.Entidades;
using Totaltech.Logica.DTOs;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IPagosLogica
    {
        Task<List<Pago>> ObtenerTodosAsync();
        Task<Pago?> ObtenerPorIdAsync(int id);
        Task<List<Pago>> ObtenerPorPedidoAsync(int idPedido);
        Task<string?> CrearParaPedidoAsync(int idPedido, Pago pago);
        Task<OperacionDominioResultado> ActualizarEstadoAsync(int id, EstadoPago estado);
    }

    public class PagosLogica : IPagosLogica
    {
        private readonly IPagosRepositorio _repositorio;
        private readonly TotaltechDbContext _context;

        public PagosLogica(
            TotaltechDbContext context,
            IPagosRepositorio repositorio)
        {
            _context = context;
            _repositorio = repositorio;
        }

        public Task<List<Pago>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<Pago?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public Task<List<Pago>> ObtenerPorPedidoAsync(int idPedido)
        {
            return _repositorio.ObtenerPorPedidoAsync(idPedido);
        }

        public async Task<string?> CrearParaPedidoAsync(int idPedido, Pago pago)
        {
            pago.IdPedido = idPedido;
            pago.Estado = EstadoPago.Pendiente;

            if (pago.Monto <= 0)
            {
                return "El monto del pago debe ser mayor a cero.";
            }

            if (!Enum.IsDefined(pago.MetodoPago))
            {
                return "El metodo del pago no es valido.";
            }

            async Task<string?> EjecutarAsync(CancellationToken cancellationToken)
            {
                _context.ChangeTracker.Clear();
                var pedido = await _context.Pedidos
                    .SingleOrDefaultAsync(candidato => candidato.IdPedido == idPedido, cancellationToken);
                if (pedido is null)
                {
                    return "El pedido indicado no existe.";
                }

                if (pedido.Estado != EstadoPedido.Pendiente)
                {
                    return "Solo se pueden registrar pagos para pedidos pendientes.";
                }

                pago.FechaPago = pago.FechaPago == default ? DateTime.UtcNow : pago.FechaPago;
                _context.Pagos.Add(pago);
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
                    return pago.IdPago > 0 && await _context.Pagos
                        .AsNoTracking()
                        .AnyAsync(candidato => candidato.IdPago == pago.IdPago, cancellationToken);
                },
                IsolationLevel.Serializable,
                CancellationToken.None);
        }

        public async Task<OperacionDominioResultado> ActualizarEstadoAsync(int id, EstadoPago estado)
        {
            if (!Enum.IsDefined(estado))
            {
                return new(EstadoOperacionDominio.Invalido, "El estado del pago no es valido.");
            }

            async Task<OperacionDominioResultado> EjecutarAsync(CancellationToken cancellationToken)
            {
                _context.ChangeTracker.Clear();
                var pago = await _context.Pagos
                    .SingleOrDefaultAsync(candidato => candidato.IdPago == id, cancellationToken);
                if (pago is null)
                {
                    return new(EstadoOperacionDominio.NoEncontrado, "El pago indicado no existe.");
                }

                if (pago.Estado == estado)
                {
                    return new(EstadoOperacionDominio.Exitoso);
                }

                if (pago.Estado != EstadoPago.Pendiente || estado == EstadoPago.Pendiente)
                {
                    return new(
                        EstadoOperacionDominio.Conflicto,
                        "Los pagos finalizados no pueden cambiar de estado.");
                }

                var pedido = await _context.Pedidos
                    .SingleOrDefaultAsync(candidato => candidato.IdPedido == pago.IdPedido, cancellationToken);
                if (pedido is null)
                {
                    return new(EstadoOperacionDominio.NoEncontrado, "El pedido indicado no existe.");
                }

                if (estado == EstadoPago.Aprobado)
                {
                    if (pedido.Estado != EstadoPedido.Pendiente)
                    {
                        return new(
                            EstadoOperacionDominio.Conflicto,
                            "Solo se pueden aprobar pagos de pedidos pendientes.");
                    }

                    var montoAprobado = await _context.Pagos
                        .Where(candidato =>
                            candidato.IdPedido == pedido.IdPedido &&
                            candidato.Estado == EstadoPago.Aprobado)
                        .SumAsync(candidato => candidato.Monto, cancellationToken);
                    var nuevoMontoAprobado = checked(montoAprobado + pago.Monto);

                    if (nuevoMontoAprobado > pedido.Total)
                    {
                        return new(
                            EstadoOperacionDominio.Conflicto,
                            "El pago supera el saldo pendiente del pedido.");
                    }

                    if (nuevoMontoAprobado == pedido.Total)
                    {
                        pedido.Estado = EstadoPedido.Pagado;
                    }
                }

                pago.Estado = estado;
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
                    return await _context.Pagos
                        .AsNoTracking()
                        .AnyAsync(
                            pago => pago.IdPago == id && pago.Estado == estado,
                            cancellationToken);
                },
                IsolationLevel.Serializable,
                CancellationToken.None);
        }

    }
}
