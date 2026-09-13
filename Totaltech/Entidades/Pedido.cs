using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Totaltech.Entidades
{
    public class Pedido
    {
        [Key]
        public int IdPedido { get; set; }

        public int? IdUsuario { get; set; }

        public int? IdCarrito { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime FechaPedido { get; set; }

        [Required]
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        [Required]
        public int IdDireccion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        public string DireccionCalle { get; set; } = string.Empty;

        [Required]
        public string DireccionNumero { get; set; } = string.Empty;

        [Required]
        public string DireccionCiudad { get; set; } = string.Empty;

        [Required]
        public string DireccionProvincia { get; set; } = string.Empty;

        [Required]
        public string DireccionCodigoPostal { get; set; } = string.Empty;

        [Required]
        public string DireccionPais { get; set; } = string.Empty;

        public Usuario? Usuario { get; set; }

        public Direccion? Direccion { get; set; }

        public Carrito? Carrito { get; set; }
    }

    public enum EstadoPedido
    {
        Pendiente = 0,
        Pagado = 1,
        Enviado = 2,
        Entregado = 3,
        Cancelado = 4
    }
}
