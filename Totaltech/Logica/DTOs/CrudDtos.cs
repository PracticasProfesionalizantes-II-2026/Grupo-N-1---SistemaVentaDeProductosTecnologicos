using Totaltech.Entidades;

namespace Totaltech.Logica.DTOs
{
    public record CrearUsuarioRequest(string Nombre, string Apellido, string Email, string Contrasena, string Telefono, DateTime? FechaRegistro, RolUsuario Rol);
    public record ActualizarUsuarioRequest(string Nombre, string Apellido, string Email, string? Contrasena, string Telefono, DateTime? FechaRegistro, RolUsuario Rol);
    public record UsuarioResponse(int IdUsuario, string Nombre, string Apellido, string Email, string Telefono, DateTime FechaRegistro, RolUsuario Rol);

    public record CrearDireccionRequest(int? IdUsuario, string Calle, string Numero, string Ciudad, string Provincia, string CodigoPostal, string Pais, TipoDireccion Tipo);
    public record ActualizarDireccionRequest(int? IdUsuario, string Calle, string Numero, string Ciudad, string Provincia, string CodigoPostal, string Pais, TipoDireccion Tipo);
    public record DireccionResponse(int IdDireccion, int? IdUsuario, string Calle, string Numero, string Ciudad, string Provincia, string CodigoPostal, string Pais, TipoDireccion Tipo);

    public record CrearProveedorRequest(string RazonSocial, string Cuit, string EmailComercial, string TelefonoComercial, string CondicionIva, int? IdDireccion, int PlazoPagoDias, int TiempoEntregaDias, string MonedaPreferida, bool Activo);
    public record ActualizarProveedorRequest(string RazonSocial, string Cuit, string EmailComercial, string TelefonoComercial, string CondicionIva, int? IdDireccion, int PlazoPagoDias, int TiempoEntregaDias, string MonedaPreferida, bool Activo);
    public record ProveedorResponse(int IdProveedor, string RazonSocial, string Cuit, string EmailComercial, string TelefonoComercial, string CondicionIva, int? IdDireccion, int PlazoPagoDias, int TiempoEntregaDias, string MonedaPreferida, bool Activo);

    public record CrearProductoRequest(string Nombre, string Descripcion, decimal Precio, int Stock, int IdCategoria, int IdProveedor);
    public record ActualizarProductoRequest(string Nombre, string Descripcion, decimal Precio, int Stock, int IdCategoria, int IdProveedor);
    public record ProductoResponse(int IdProducto, string Nombre, string Descripcion, decimal Precio, int Stock, int IdCategoria, int IdProveedor);

    public record CrearCategoriaRequest(string Nombre, string Descripcion);
    public record ActualizarCategoriaRequest(string Nombre, string Descripcion);
    public record CategoriaResponse(int IdCategoria, string Nombre, string Descripcion);

    public record CrearPedidoRequest(int? IdUsuario, DateTime? FechaPedido, EstadoPedido Estado, int IdDireccion);
    public record ActualizarPedidoRequest(int? IdUsuario, DateTime? FechaPedido, EstadoPedido Estado, int IdDireccion);
    public record PedidoResponse(int IdPedido, int? IdUsuario, DateTime FechaPedido, EstadoPedido Estado, int IdDireccion);

    public record CrearDetallePedidoRequest(int IdPedido, int IdProducto, int Cantidad, decimal PrecioUnitario, decimal Subtotal);
    public record ActualizarDetallePedidoRequest(int IdPedido, int IdProducto, int Cantidad, decimal PrecioUnitario, decimal Subtotal);
    public record DetallePedidoResponse(int IdDetallePedido, int IdPedido, int IdProducto, int Cantidad, decimal PrecioUnitario, decimal Subtotal);

    public record CrearCarritoRequest(int IdUsuario, DateTime? FechaCreacion, EstadoCarrito Estado);
    public record ActualizarCarritoRequest(int IdUsuario, DateTime? FechaCreacion, EstadoCarrito Estado);
    public record CarritoResponse(int IdCarrito, int IdUsuario, DateTime FechaCreacion, EstadoCarrito Estado);

    public record CrearDetalleCarritoRequest(int IdCarrito, int IdProducto, int Cantidad, decimal PrecioUnitario, decimal Subtotal);
    public record ActualizarDetalleCarritoRequest(int IdCarrito, int IdProducto, int Cantidad, decimal PrecioUnitario, decimal Subtotal);
    public record DetalleCarritoResponse(int IdDetalleCarrito, int IdCarrito, int IdProducto, int Cantidad, decimal PrecioUnitario, decimal Subtotal);

    public record CrearPagoRequest(int IdPedido, DateTime? FechaPago, MetodoPago MetodoPago, decimal Monto, EstadoPago Estado);
    public record CrearPagoParaPedidoRequest(DateTime? FechaPago, MetodoPago MetodoPago, decimal Monto, EstadoPago Estado);
    public record ActualizarPagoRequest(int IdPedido, DateTime? FechaPago, MetodoPago MetodoPago, decimal Monto, EstadoPago Estado);
    public record PagoResponse(int IdPago, int IdPedido, DateTime FechaPago, MetodoPago MetodoPago, decimal Monto, EstadoPago Estado);

    public record CrearCompraRequest(int IdProveedor, DateTime? FechaCompra, decimal Total, EstadoCompra Estado);
    public record ActualizarCompraRequest(int IdProveedor, DateTime? FechaCompra, decimal Total, EstadoCompra Estado);
    public record CompraResponse(int IdCompra, int IdProveedor, DateTime FechaCompra, decimal Total, EstadoCompra Estado);

    public record CrearReporteRequest(TipoReporte TipoReporte, DateTime? FechaInicio, DateTime? FechaFin, int IdUsuario);
    public record ActualizarReporteRequest(TipoReporte TipoReporte, DateTime? FechaInicio, DateTime? FechaFin, int IdUsuario);
    public record ReporteResponse(int IdReporte, TipoReporte TipoReporte, DateTime FechaInicio, DateTime FechaFin, int IdUsuario);

    public record CrearConsultaRequest(int? IdUsuario, string Nombre, string Email, string Mensaje, DateTime? FechaConsulta, EstadoConsulta Estado);
    public record ActualizarConsultaRequest(int? IdUsuario, string Nombre, string Email, string Mensaje, DateTime? FechaConsulta, EstadoConsulta Estado);
    public record ConsultaResponse(int IdConsulta, int? IdUsuario, string Nombre, string Email, string Mensaje, DateTime FechaConsulta, EstadoConsulta Estado);

    public record ActualizarStockRequest(int Stock);
    public record ActualizarEstadoPedidoRequest(EstadoPedido Estado);
    public record ActualizarEstadoPagoRequest(EstadoPago Estado);
}
