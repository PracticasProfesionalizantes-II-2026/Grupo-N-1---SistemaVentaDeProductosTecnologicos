using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IDetallePedidosLogica
    {
        Task<List<DetallePedido>> ObtenerTodosAsync();
        Task<DetallePedido?> ObtenerPorIdAsync(int id);
        Task<List<DetallePedido>> ObtenerPorPedidoAsync(int idPedido);
    }

    public class DetallePedidosLogica : IDetallePedidosLogica
    {
        private readonly IDetallePedidosRepositorio _repositorio;

        public DetallePedidosLogica(IDetallePedidosRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public Task<List<DetallePedido>> ObtenerTodosAsync()
        {
            return _repositorio.ObtenerTodosAsync();
        }

        public Task<DetallePedido?> ObtenerPorIdAsync(int id)
        {
            return _repositorio.ObtenerPorIdAsync(id);
        }

        public Task<List<DetallePedido>> ObtenerPorPedidoAsync(int idPedido)
        {
            return _repositorio.ObtenerPorPedidoAsync(idPedido);
        }

    }
}
