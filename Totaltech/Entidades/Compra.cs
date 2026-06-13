using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Totaltech.Entidades
{
    public class Compra
    {
        [Key]
        public int IdCompra { get; set; }

        public int IdProveedor { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaCompra { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        public EstadoCompra Estado { get; set; } = EstadoCompra.Pendiente;

        public Proveedor? Proveedor { get; set; }
    }

    public enum EstadoCompra
    {
        Pendiente = 0,
        Confirmada = 1,
        Cancelada = 2
    }
}
