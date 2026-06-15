using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IPagosLogica : ILogica<Pago>
    {
        Task<bool> ActualizarEstadoAsync(int id, EstadoPago estado);
        Task<List<Pago>> ObtenerPorPedidoAsync(int idPedido);
        Task<Pago?> CrearParaPedidoAsync(int idPedido, Pago pago);
    }

    public class PagosLogica : Logica<Pago>, IPagosLogica
    {
        private readonly IPagosRepositorio _repositorio;
        private readonly IPedidosRepositorio _pedidosRepositorio;

        public PagosLogica(IPagosRepositorio repositorio, IPedidosRepositorio pedidosRepositorio) : base(repositorio)
        {
            _repositorio = repositorio;
            _pedidosRepositorio = pedidosRepositorio;
        }

        public async Task<bool> ActualizarEstadoAsync(int id, EstadoPago estado)
        {
            var pago = await _repositorio.ObtenerPorIdAsync(id);

            if (pago is null)
            {
                return false;
            }

            pago.Estado = estado;
            await _repositorio.ActualizarAsync(pago);
            return true;
        }

        public Task<List<Pago>> ObtenerPorPedidoAsync(int idPedido)
        {
            return _repositorio.ObtenerPorPedidoAsync(idPedido);
        }

        public async Task<Pago?> CrearParaPedidoAsync(int idPedido, Pago pago)
        {
            var pedido = await _pedidosRepositorio.ObtenerPorIdAsync(idPedido);

            if (pedido is null)
            {
                return null;
            }

            pago.IdPedido = idPedido;

            if (pago.FechaPago == default)
            {
                pago.FechaPago = DateTime.Now;
            }

            await _repositorio.CrearAsync(pago);
            return pago;
        }
    }
}
