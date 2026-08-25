using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IPagosLogica
    {
        Task<List<Pago>> ObtenerTodosAsync();
        Task<Pago?> ObtenerPorIdAsync(int id);
        Task<string?> CrearAsync(Pago pago);
        Task<string?> ActualizarAsync(Pago pago);
        Task<bool> EliminarAsync(int id);
        Task<List<Pago>> ObtenerPorPedidoAsync(int idPedido);
        Task<string?> CrearParaPedidoAsync(int idPedido, Pago pago);
        Task<string?> ActualizarEstadoAsync(int id, EstadoPago estado);
    }

    public class PagosLogica : IPagosLogica
    {
        private readonly IPagosRepositorio _repositorio;
        private readonly IPedidosRepositorio _pedidosRepositorio;

        public PagosLogica(IPagosRepositorio repositorio, IPedidosRepositorio pedidosRepositorio)
        {
            _repositorio = repositorio;
            _pedidosRepositorio = pedidosRepositorio;
        }

        public Task<List<Pago>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<Pago?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<string?> CrearAsync(Pago pago)
        {
            var error = await ValidarPagoAsync(pago);
            if (error is not null)
            {
                return error;
            }

            if (pago.FechaPago == default)
            {
                pago.FechaPago = DateTime.Now;
            }

            await _repositorio.CrearAsync(pago);
            await SincronizarEstadoPedidoAsync(pago.IdPedido);
            return null;
        }

        public async Task<string?> ActualizarAsync(Pago pago)
        {
            var error = await ValidarPagoAsync(pago);
            if (error is not null)
            {
                return error;
            }

            if (pago.FechaPago == default)
            {
                pago.FechaPago = DateTime.Now;
            }

            await _repositorio.ActualizarAsync(pago);
            await SincronizarEstadoPedidoAsync(pago.IdPedido);
            return null;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var pago = await _repositorio.ObtenerPorIdAsync(id);
            if (pago is null)
            {
                return false;
            }

            var idPedido = pago.IdPedido;
            await _repositorio.EliminarAsync(pago);
            await SincronizarEstadoPedidoAsync(idPedido);
            return true;
        }

        public Task<List<Pago>> ObtenerPorPedidoAsync(int idPedido)
        {
            return _repositorio.ObtenerPorPedidoAsync(idPedido);
        }

        public Task<string?> CrearParaPedidoAsync(int idPedido, Pago pago)
        {
            pago.IdPedido = idPedido;
            return CrearAsync(pago);
        }

        public async Task<string?> ActualizarEstadoAsync(int id, EstadoPago estado)
        {
            if (!Enum.IsDefined(estado))
            {
                return "El estado del pago no es valido.";
            }

            var pago = await _repositorio.ObtenerPorIdAsync(id);
            if (pago is null)
            {
                return "El pago indicado no existe.";
            }

            pago.Estado = estado;
            await _repositorio.ActualizarAsync(pago);
            await SincronizarEstadoPedidoAsync(pago.IdPedido);
            return null;
        }

        private async Task<string?> ValidarPagoAsync(Pago pago)
        {
            if (pago.Monto <= 0)
            {
                return "El monto del pago debe ser mayor a cero.";
            }

            if (!Enum.IsDefined(pago.MetodoPago) || !Enum.IsDefined(pago.Estado))
            {
                return "El metodo o el estado del pago no es valido.";
            }

            if (!await _pedidosRepositorio.ExisteAsync(pago.IdPedido))
            {
                return "El pedido indicado no existe.";
            }

            return null;
        }

        private async Task SincronizarEstadoPedidoAsync(int idPedido)
        {
            var pedido = await _pedidosRepositorio.ObtenerPorIdAsync(idPedido);
            if (pedido is null || pedido.Estado == EstadoPedido.Cancelado)
            {
                return;
            }

            var pagos = await _repositorio.ObtenerPorPedidoAsync(idPedido);
            var tienePagoAprobado = pagos.Any(pago => pago.Estado == EstadoPago.Aprobado);

            if (tienePagoAprobado && pedido.Estado == EstadoPedido.Pendiente)
            {
                pedido.Estado = EstadoPedido.Pagado;
                await _pedidosRepositorio.ActualizarAsync(pedido);
            }
            else if (!tienePagoAprobado && pedido.Estado == EstadoPedido.Pagado)
            {
                pedido.Estado = EstadoPedido.Pendiente;
                await _pedidosRepositorio.ActualizarAsync(pedido);
            }
        }
    }
}
