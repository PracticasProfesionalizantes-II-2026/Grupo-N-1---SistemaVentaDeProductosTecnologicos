using Totaltech.Datos;
using Totaltech.Entidades;

namespace Totaltech.Repositorios
{
    public interface IDetallePedidosRepositorio : IRepositorio<DetallePedido>
    {
    }

    public class DetallePedidosRepositorio : Repositorio<DetallePedido>, IDetallePedidosRepositorio
    {
        public DetallePedidosRepositorio(TotaltechDbContext context) : base(context)
        {
        }
    }
}
