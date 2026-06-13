using System.ComponentModel.DataAnnotations;

namespace Totaltech.Entidades
{
    public class Pedido
    {
        [Key]
        public int IdPedido { get; set; }

        public int? IdUsuario { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime FechaPedido { get; set; }

        [Required]
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        [Required]
        public int IdDireccion { get; set; }

        public Usuario? Usuario { get; set; }

        public Direccion? Direccion { get; set; }
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
