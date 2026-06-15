using Totaltech.Entidades;
using Totaltech.Logica;

namespace Totaltech.Endpoints
{
    public static class DetallePedidosEndpoints
    {
        public static void MapDetallePedidosEndpoints(this WebApplication app)
        {
            app.MapCrud<DetallePedido, IDetallePedidosLogica>("/detallepedidos", "Detalle pedidos", detalle => detalle.IdDetallePedido);
        }
    }
}
