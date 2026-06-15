using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Totaltech.Entidades
{
    public class DetalleCarrito
    {
        [Key]
        public int IdDetalleCarrito { get; set; }

        public int IdCarrito { get; set; }

        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        public Carrito? Carrito { get; set; }

        public Producto? Producto { get; set; }
    }
}
