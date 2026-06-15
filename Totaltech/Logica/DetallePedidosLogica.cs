using Totaltech.Entidades;
using Totaltech.Repositorios;

namespace Totaltech.Logica
{
    public interface IDetallePedidosLogica : ILogica<DetallePedido>
    {
    }

    public class DetallePedidosLogica : Logica<DetallePedido>, IDetallePedidosLogica
    {
        public DetallePedidosLogica(IDetallePedidosRepositorio repositorio) : base(repositorio)
        {
        }
    }
}
