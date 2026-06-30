using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Totaltech.Entidades
{
    public class DetallePedido
    {
        [Key]
        public int IdDetallePedido { get; set; }

        public int IdPedido { get; set; }

        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        public Pedido? Pedido { get; set; }

        public Producto? Producto { get; set; }
    }
}
