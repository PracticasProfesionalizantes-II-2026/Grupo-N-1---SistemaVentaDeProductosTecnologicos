using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IPedidosLogica : ILogica<Pedido>
    {
        Task<List<Pedido>> ObtenerPorUsuarioAsync(int idUsuario);
        Task<List<Pedido>> ObtenerPorEstadoAsync(EstadoPedido estado);
        Task<bool> ActualizarEstadoAsync(int id, EstadoPedido estado);
    }

    public class PedidosLogica : Logica<Pedido>, IPedidosLogica
    {
        private readonly IPedidosRepositorio _repositorio;

        public PedidosLogica(IPedidosRepositorio repositorio) : base(repositorio)
        {
            _repositorio = repositorio;
        }

        public Task<List<Pedido>> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return _repositorio.ObtenerPorUsuarioAsync(idUsuario);
        }

        public Task<List<Pedido>> ObtenerPorEstadoAsync(EstadoPedido estado)
        {
            return _repositorio.ObtenerPorEstadoAsync(estado);
        }

        public async Task<bool> ActualizarEstadoAsync(int id, EstadoPedido estado)
        {
            var pedido = await _repositorio.ObtenerPorIdAsync(id);

            if (pedido is null)
            {
                return false;
            }

            pedido.Estado = estado;
            await _repositorio.ActualizarAsync(pedido);
            return true;
        }
    }
}
