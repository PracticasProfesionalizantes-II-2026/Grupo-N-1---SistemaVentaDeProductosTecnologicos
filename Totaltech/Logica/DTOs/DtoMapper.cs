using Totaltech.Entidades;

namespace Totaltech.Logica.DTOs
{
    public static class DtoMapper
    {
        public static Usuario ToEntity(this CrearUsuarioRequest request)
        {
            return new Usuario { Nombre = request.Nombre, Apellido = request.Apellido, Email = request.Email, Contrasena = request.Contrasena, Telefono = request.Telefono, FechaRegistro = request.FechaRegistro ?? default, Rol = request.Rol };
        }

        public static Usuario ToEntity(this ActualizarUsuarioRequest request, int id)
        {
            return new Usuario { IdUsuario = id, Nombre = request.Nombre, Apellido = request.Apellido, Email = request.Email, Contrasena = request.Contrasena ?? string.Empty, Telefono = request.Telefono, FechaRegistro = request.FechaRegistro ?? default, Rol = request.Rol };
        }

        public static UsuarioResponse ToResponse(this Usuario usuario)
        {
            return new UsuarioResponse(usuario.IdUsuario, usuario.Nombre, usuario.Apellido, usuario.Email, usuario.Telefono, usuario.FechaRegistro, usuario.Rol);
        }

        public static Direccion ToEntity(this CrearDireccionRequest request)
        {
            return new Direccion { IdUsuario = request.IdUsuario, Calle = request.Calle, Numero = request.Numero, Ciudad = request.Ciudad, Provincia = request.Provincia, CodigoPostal = request.CodigoPostal, Pais = request.Pais, Tipo = request.Tipo };
        }

        public static Direccion ToEntity(this ActualizarDireccionRequest request, int id)
        {
            return new Direccion { IdDireccion = id, IdUsuario = request.IdUsuario, Calle = request.Calle, Numero = request.Numero, Ciudad = request.Ciudad, Provincia = request.Provincia, CodigoPostal = request.CodigoPostal, Pais = request.Pais, Tipo = request.Tipo };
        }

        public static DireccionResponse ToResponse(this Direccion direccion)
        {
            return new DireccionResponse(direccion.IdDireccion, direccion.IdUsuario, direccion.Calle, direccion.Numero, direccion.Ciudad, direccion.Provincia, direccion.CodigoPostal, direccion.Pais, direccion.Tipo);
        }

        public static Proveedor ToEntity(this CrearProveedorRequest request)
        {
            return new Proveedor { RazonSocial = request.RazonSocial, Cuit = request.Cuit, EmailComercial = request.EmailComercial, TelefonoComercial = request.TelefonoComercial, CondicionIva = request.CondicionIva, IdDireccion = request.IdDireccion, PlazoPagoDias = request.PlazoPagoDias, TiempoEntregaDias = request.TiempoEntregaDias, MonedaPreferida = request.MonedaPreferida, Activo = request.Activo };
        }

        public static Proveedor ToEntity(this ActualizarProveedorRequest request, int id)
        {
            return new Proveedor { IdProveedor = id, RazonSocial = request.RazonSocial, Cuit = request.Cuit, EmailComercial = request.EmailComercial, TelefonoComercial = request.TelefonoComercial, CondicionIva = request.CondicionIva, IdDireccion = request.IdDireccion, PlazoPagoDias = request.PlazoPagoDias, TiempoEntregaDias = request.TiempoEntregaDias, MonedaPreferida = request.MonedaPreferida, Activo = request.Activo };
        }

        public static ProveedorResponse ToResponse(this Proveedor proveedor)
        {
            return new ProveedorResponse(proveedor.IdProveedor, proveedor.RazonSocial, proveedor.Cuit, proveedor.EmailComercial, proveedor.TelefonoComercial, proveedor.CondicionIva, proveedor.IdDireccion, proveedor.PlazoPagoDias, proveedor.TiempoEntregaDias, proveedor.MonedaPreferida, proveedor.Activo);
        }

        public static Producto ToEntity(this CrearProductoRequest request)
        {
            return new Producto { Nombre = request.Nombre, Descripcion = request.Descripcion, Precio = request.Precio, Stock = request.Stock, IdCategoria = request.IdCategoria, IdProveedor = request.IdProveedor };
        }

        public static Producto ToEntity(this ActualizarProductoRequest request, int id)
        {
            return new Producto { IdProducto = id, Nombre = request.Nombre, Descripcion = request.Descripcion, Precio = request.Precio, Stock = request.Stock, IdCategoria = request.IdCategoria, IdProveedor = request.IdProveedor };
        }

        public static ProductoResponse ToResponse(this Producto producto)
        {
            return new ProductoResponse(producto.IdProducto, producto.Nombre, producto.Descripcion, producto.Precio, producto.Stock, producto.IdCategoria, producto.IdProveedor);
        }

        public static Categoria ToEntity(this CrearCategoriaRequest request)
        {
            return new Categoria { Nombre = request.Nombre, Descripcion = request.Descripcion };
        }

        public static Categoria ToEntity(this ActualizarCategoriaRequest request, int id)
        {
            return new Categoria { IdCategoria = id, Nombre = request.Nombre, Descripcion = request.Descripcion };
        }

        public static CategoriaResponse ToResponse(this Categoria categoria)
        {
            return new CategoriaResponse(categoria.IdCategoria, categoria.Nombre, categoria.Descripcion);
        }

        public static Pedido ToEntity(this CrearPedidoRequest request)
        {
            return new Pedido { IdUsuario = request.IdUsuario, FechaPedido = request.FechaPedido ?? default, Estado = request.Estado, IdDireccion = request.IdDireccion };
        }

        public static Pedido ToEntity(this ActualizarPedidoRequest request, int id)
        {
            return new Pedido { IdPedido = id, IdUsuario = request.IdUsuario, FechaPedido = request.FechaPedido ?? default, Estado = request.Estado, IdDireccion = request.IdDireccion };
        }

        public static PedidoResponse ToResponse(this Pedido pedido)
        {
            return new PedidoResponse(pedido.IdPedido, pedido.IdUsuario, pedido.FechaPedido, pedido.Estado, pedido.IdDireccion);
        }

        public static DetallePedido ToEntity(this CrearDetallePedidoRequest request)
        {
            return new DetallePedido { IdPedido = request.IdPedido, IdProducto = request.IdProducto, Cantidad = request.Cantidad, PrecioUnitario = request.PrecioUnitario, Subtotal = request.Subtotal };
        }

        public static DetallePedido ToEntity(this ActualizarDetallePedidoRequest request, int id)
        {
            return new DetallePedido { IdDetallePedido = id, IdPedido = request.IdPedido, IdProducto = request.IdProducto, Cantidad = request.Cantidad, PrecioUnitario = request.PrecioUnitario, Subtotal = request.Subtotal };
        }

        public static DetallePedidoResponse ToResponse(this DetallePedido detalle)
        {
            return new DetallePedidoResponse(detalle.IdDetallePedido, detalle.IdPedido, detalle.IdProducto, detalle.Cantidad, detalle.PrecioUnitario, detalle.Subtotal);
        }

        public static Carrito ToEntity(this CrearCarritoRequest request)
        {
            return new Carrito { IdUsuario = request.IdUsuario, FechaCreacion = request.FechaCreacion ?? default, Estado = request.Estado };
        }

        public static Carrito ToEntity(this ActualizarCarritoRequest request, int id)
        {
            return new Carrito { IdCarrito = id, IdUsuario = request.IdUsuario, FechaCreacion = request.FechaCreacion ?? default, Estado = request.Estado };
        }

        public static CarritoResponse ToResponse(this Carrito carrito)
        {
            return new CarritoResponse(carrito.IdCarrito, carrito.IdUsuario, carrito.FechaCreacion, carrito.Estado);
        }

        public static DetalleCarrito ToEntity(this CrearDetalleCarritoRequest request)
        {
            return new DetalleCarrito { IdCarrito = request.IdCarrito, IdProducto = request.IdProducto, Cantidad = request.Cantidad, PrecioUnitario = request.PrecioUnitario, Subtotal = request.Subtotal };
        }

        public static DetalleCarrito ToEntity(this ActualizarDetalleCarritoRequest request, int id)
        {
            return new DetalleCarrito { IdDetalleCarrito = id, IdCarrito = request.IdCarrito, IdProducto = request.IdProducto, Cantidad = request.Cantidad, PrecioUnitario = request.PrecioUnitario, Subtotal = request.Subtotal };
        }

        public static DetalleCarritoResponse ToResponse(this DetalleCarrito detalle)
        {
            return new DetalleCarritoResponse(detalle.IdDetalleCarrito, detalle.IdCarrito, detalle.IdProducto, detalle.Cantidad, detalle.PrecioUnitario, detalle.Subtotal);
        }

        public static Pago ToEntity(this CrearPagoRequest request)
        {
            return new Pago { IdPedido = request.IdPedido, FechaPago = request.FechaPago ?? default, MetodoPago = request.MetodoPago, Monto = request.Monto, Estado = request.Estado };
        }

        public static Pago ToEntity(this CrearPagoParaPedidoRequest request, int idPedido)
        {
            return new Pago { IdPedido = idPedido, FechaPago = request.FechaPago ?? default, MetodoPago = request.MetodoPago, Monto = request.Monto, Estado = request.Estado };
        }

        public static Pago ToEntity(this ActualizarPagoRequest request, int id)
        {
            return new Pago { IdPago = id, IdPedido = request.IdPedido, FechaPago = request.FechaPago ?? default, MetodoPago = request.MetodoPago, Monto = request.Monto, Estado = request.Estado };
        }

        public static PagoResponse ToResponse(this Pago pago)
        {
            return new PagoResponse(pago.IdPago, pago.IdPedido, pago.FechaPago, pago.MetodoPago, pago.Monto, pago.Estado);
        }

        public static Compra ToEntity(this CrearCompraRequest request)
        {
            return new Compra { IdProveedor = request.IdProveedor, FechaCompra = request.FechaCompra ?? default, Total = request.Total, Estado = request.Estado };
        }

        public static Compra ToEntity(this ActualizarCompraRequest request, int id)
        {
            return new Compra { IdCompra = id, IdProveedor = request.IdProveedor, FechaCompra = request.FechaCompra ?? default, Total = request.Total, Estado = request.Estado };
        }

        public static CompraResponse ToResponse(this Compra compra)
        {
            return new CompraResponse(compra.IdCompra, compra.IdProveedor, compra.FechaCompra, compra.Total, compra.Estado);
        }

        public static Reporte ToEntity(this CrearReporteRequest request)
        {
            return new Reporte { TipoReporte = request.TipoReporte, FechaInicio = request.FechaInicio ?? default, FechaFin = request.FechaFin ?? default, IdUsuario = request.IdUsuario };
        }

        public static Reporte ToEntity(this ActualizarReporteRequest request, int id)
        {
            return new Reporte { IdReporte = id, TipoReporte = request.TipoReporte, FechaInicio = request.FechaInicio ?? default, FechaFin = request.FechaFin ?? default, IdUsuario = request.IdUsuario };
        }

        public static ReporteResponse ToResponse(this Reporte reporte)
        {
            return new ReporteResponse(reporte.IdReporte, reporte.TipoReporte, reporte.FechaInicio, reporte.FechaFin, reporte.IdUsuario);
        }

        public static Consulta ToEntity(this CrearConsultaRequest request)
        {
            return new Consulta { IdUsuario = request.IdUsuario, Nombre = request.Nombre, Email = request.Email, Mensaje = request.Mensaje, FechaConsulta = request.FechaConsulta ?? default, Estado = request.Estado };
        }

        public static Consulta ToEntity(this ActualizarConsultaRequest request, int id)
        {
            return new Consulta { IdConsulta = id, IdUsuario = request.IdUsuario, Nombre = request.Nombre, Email = request.Email, Mensaje = request.Mensaje, FechaConsulta = request.FechaConsulta ?? default, Estado = request.Estado };
        }

        public static ConsultaResponse ToResponse(this Consulta consulta)
        {
            return new ConsultaResponse(consulta.IdConsulta, consulta.IdUsuario, consulta.Nombre, consulta.Email, consulta.Mensaje, consulta.FechaConsulta, consulta.Estado);
        }
    }
}
