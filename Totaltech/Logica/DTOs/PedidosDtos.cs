using Totaltech.Entidades;

namespace Totaltech.Logica.DTOs;

public sealed class DireccionPedidoResponse
{
    public string Calle { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string Ciudad { get; init; } = string.Empty;
    public string Provincia { get; init; } = string.Empty;
    public string CodigoPostal { get; init; } = string.Empty;
    public string Pais { get; init; } = string.Empty;
}

public sealed class DetallePedidoResponse
{
    public int IdDetallePedido { get; init; }
    public int IdProducto { get; init; }
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal Subtotal { get; init; }
}

public sealed class PedidoResponse
{
    public int IdPedido { get; init; }
    public int? IdCarrito { get; init; }
    public int? IdUsuario { get; init; }
    public DateTime FechaPedido { get; init; }
    public EstadoPedido Estado { get; init; }
    public decimal Total { get; init; }
    public int IdDireccion { get; init; }
    public DireccionPedidoResponse Direccion { get; init; } = new();
    public IReadOnlyCollection<DetallePedidoResponse> Detalles { get; init; } = [];
}

public sealed class PagoResponse
{
    public int IdPago { get; init; }
    public int IdPedido { get; init; }
    public DateTime FechaPago { get; init; }
    public MetodoPago MetodoPago { get; init; }
    public decimal Monto { get; init; }
    public EstadoPago Estado { get; init; }
}

public enum EstadoConfirmacionCarrito
{
    Creado,
    Repetido,
    NoEncontrado,
    Conflicto,
    Invalido
}

public sealed record ConfirmarCarritoResultado(
    EstadoConfirmacionCarrito Estado,
    Pedido? Pedido = null,
    string? Error = null);

public enum EstadoOperacionDominio
{
    Exitoso,
    NoEncontrado,
    Conflicto,
    Invalido
}

public sealed record OperacionDominioResultado(
    EstadoOperacionDominio Estado,
    string? Error = null);

public static class PedidosDtoMapper
{
    public static PedidoResponse ToResponse(
        this Pedido pedido,
        IEnumerable<DetallePedido>? detalles = null) => new()
    {
        IdPedido = pedido.IdPedido,
        IdCarrito = pedido.IdCarrito,
        IdUsuario = pedido.IdUsuario,
        FechaPedido = pedido.FechaPedido,
        Estado = pedido.Estado,
        Total = pedido.Total,
        IdDireccion = pedido.IdDireccion,
        Direccion = new DireccionPedidoResponse
        {
            Calle = pedido.DireccionCalle,
            Numero = pedido.DireccionNumero,
            Ciudad = pedido.DireccionCiudad,
            Provincia = pedido.DireccionProvincia,
            CodigoPostal = pedido.DireccionCodigoPostal,
            Pais = pedido.DireccionPais
        },
        Detalles = (detalles ?? []).Select(detalle => detalle.ToResponse()).ToArray()
    };

    public static DetallePedidoResponse ToResponse(this DetallePedido detalle) => new()
    {
        IdDetallePedido = detalle.IdDetallePedido,
        IdProducto = detalle.IdProducto,
        Cantidad = detalle.Cantidad,
        PrecioUnitario = detalle.PrecioUnitario,
        Subtotal = detalle.Subtotal
    };

    public static PagoResponse ToResponse(this Pago pago) => new()
    {
        IdPago = pago.IdPago,
        IdPedido = pago.IdPedido,
        FechaPago = pago.FechaPago,
        MetodoPago = pago.MetodoPago,
        Monto = pago.Monto,
        Estado = pago.Estado
    };
}
