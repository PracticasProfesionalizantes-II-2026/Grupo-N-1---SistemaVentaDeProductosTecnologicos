using Totaltech.Entidades;

namespace Totaltech.Logica.DTOs
{
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
