using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Totaltech.Entidades
{
    public class Pago
    {
        [Key]
        public int IdPago { get; set; }

        public int IdPedido { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaPago { get; set; }

        [Required]
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Tarjeta;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Required]
        public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

        public Pedido? Pedido { get; set; }
    }

    public enum MetodoPago
    {
        Tarjeta = 0,
        MercadoPago = 1,
        Transferencia = 2,
        Efectivo = 3
    }

    public enum EstadoPago
    {
        Pendiente = 0,
        Aprobado = 1,
        Rechazado = 2,
        Cancelado = 3
    }
}
