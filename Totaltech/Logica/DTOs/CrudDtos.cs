using Totaltech.Entidades;

namespace Totaltech.Logica.DTOs
{
    // Estos contratos se usan como cuerpos de POST y PUT. No incluyen las claves
    // primarias porque SQL Server las genera al insertar cada entidad.
    public class UsuarioRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public RolUsuario Rol { get; set; } = RolUsuario.Cliente;

        public Usuario ToEntity() => new()
        {
            Nombre = Nombre,
            Apellido = Apellido,
            Email = Email,
            Contrasena = Contrasena,
            Telefono = Telefono,
            FechaRegistro = FechaRegistro,
            Rol = Rol
        };
    }

    public class DireccionRequest
    {
        public int? IdUsuario { get; set; }
        public string Calle { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public TipoDireccion Tipo { get; set; } = TipoDireccion.Envio;

        public Direccion ToEntity() => new()
        {
            IdUsuario = IdUsuario,
            Calle = Calle,
            Numero = Numero,
            Ciudad = Ciudad,
            Provincia = Provincia,
            CodigoPostal = CodigoPostal,
            Pais = Pais,
            Tipo = Tipo
        };
    }

    public class ProveedorRequest
    {
        public string RazonSocial { get; set; } = string.Empty;
        public string Cuit { get; set; } = string.Empty;
        public string EmailComercial { get; set; } = string.Empty;
        public string TelefonoComercial { get; set; } = string.Empty;
        public string CondicionIva { get; set; } = string.Empty;
        public int? IdDireccion { get; set; }
        public int PlazoPagoDias { get; set; }
        public int TiempoEntregaDias { get; set; }
        public string MonedaPreferida { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        public Proveedor ToEntity() => new()
        {
            RazonSocial = RazonSocial,
            Cuit = Cuit,
            EmailComercial = EmailComercial,
            TelefonoComercial = TelefonoComercial,
            CondicionIva = CondicionIva,
            IdDireccion = IdDireccion,
            PlazoPagoDias = PlazoPagoDias,
            TiempoEntregaDias = TiempoEntregaDias,
            MonedaPreferida = MonedaPreferida,
            Activo = Activo
        };
    }

    public class ProductoRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int IdCategoria { get; set; }
        public int IdProveedor { get; set; }

        public Producto ToEntity() => new()
        {
            Nombre = Nombre,
            Descripcion = Descripcion,
            Precio = Precio,
            Stock = Stock,
            IdCategoria = IdCategoria,
            IdProveedor = IdProveedor
        };
    }

    public class CategoriaRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public Categoria ToEntity() => new()
        {
            Nombre = Nombre,
            Descripcion = Descripcion
        };
    }

    public class PedidoRequest
    {
        public int? IdUsuario { get; set; }
        public DateTime FechaPedido { get; set; }
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;
        public int IdDireccion { get; set; }

        public Pedido ToEntity() => new()
        {
            IdUsuario = IdUsuario,
            FechaPedido = FechaPedido,
            Estado = Estado,
            IdDireccion = IdDireccion
        };
    }

    public class DetallePedidoRequest
    {
        public int IdPedido { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public DetallePedido ToEntity() => new()
        {
            IdPedido = IdPedido,
            IdProducto = IdProducto,
            Cantidad = Cantidad,
            PrecioUnitario = PrecioUnitario
        };
    }

    public class CarritoRequest
    {
        public int IdUsuario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public EstadoCarrito Estado { get; set; } = EstadoCarrito.Activo;

        public Carrito ToEntity() => new()
        {
            IdUsuario = IdUsuario,
            FechaCreacion = FechaCreacion,
            Estado = Estado
        };
    }

    public class DetalleCarritoRequest
    {
        public int IdCarrito { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public DetalleCarrito ToEntity() => new()
        {
            IdCarrito = IdCarrito,
            IdProducto = IdProducto,
            Cantidad = Cantidad,
            PrecioUnitario = PrecioUnitario
        };
    }

    public class PagoRequest
    {
        public int IdPedido { get; set; }
        public DateTime FechaPago { get; set; }
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Tarjeta;
        public decimal Monto { get; set; }
        public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

        public Pago ToEntity() => new()
        {
            IdPedido = IdPedido,
            FechaPago = FechaPago,
            MetodoPago = MetodoPago,
            Monto = Monto,
            Estado = Estado
        };
    }

    public class CompraRequest
    {
        public int IdProveedor { get; set; }
        public DateTime FechaCompra { get; set; }
        public decimal Total { get; set; }
        public EstadoCompra Estado { get; set; } = EstadoCompra.Pendiente;

        public Compra ToEntity() => new()
        {
            IdProveedor = IdProveedor,
            FechaCompra = FechaCompra,
            Total = Total,
            Estado = Estado
        };
    }

    public class ReporteRequest
    {
        public TipoReporte TipoReporte { get; set; } = TipoReporte.Ventas;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int IdUsuario { get; set; }

        public Reporte ToEntity() => new()
        {
            TipoReporte = TipoReporte,
            FechaInicio = FechaInicio,
            FechaFin = FechaFin,
            IdUsuario = IdUsuario
        };
    }

    public class ConsultaRequest
    {
        public int? IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaConsulta { get; set; }
        public EstadoConsulta Estado { get; set; } = EstadoConsulta.Pendiente;

        public Consulta ToEntity() => new()
        {
            IdUsuario = IdUsuario,
            Nombre = Nombre,
            Email = Email,
            Mensaje = Mensaje,
            FechaConsulta = FechaConsulta,
            Estado = Estado
        };
    }

    public class UsuarioResponse
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public RolUsuario Rol { get; set; }
    }

    public class LoginResponse : UsuarioResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }

    public class CrearPagoParaPedidoRequest
    {
        public DateTime? FechaPago { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public decimal Monto { get; set; }
        public EstadoPago Estado { get; set; }
    }

    public class ActualizarStockRequest
    {
        public int Stock { get; set; }
    }

    public class ActualizarEstadoPedidoRequest
    {
        public EstadoPedido Estado { get; set; }
    }

    public class ActualizarEstadoPagoRequest
    {
        public EstadoPago Estado { get; set; }
    }
}
