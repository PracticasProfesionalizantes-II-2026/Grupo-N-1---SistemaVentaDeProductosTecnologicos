using Totaltech.Entidades;

namespace Totaltech.Endpoints
{
    public record ActualizarStockRequest(int Stock);

    public record ActualizarEstadoPedidoRequest(EstadoPedido Estado);

    public record ActualizarEstadoPagoRequest(EstadoPago Estado);
}
