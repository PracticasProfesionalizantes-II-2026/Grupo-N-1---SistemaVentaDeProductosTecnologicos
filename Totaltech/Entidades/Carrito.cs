using System.ComponentModel.DataAnnotations;

namespace Totaltech.Entidades
{
    public class Carrito
    {
        [Key]
        public int IdCarrito { get; set; }

        public int IdUsuario { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaCreacion { get; set; }

        [Required]
        public EstadoCarrito Estado { get; set; } = EstadoCarrito.Activo;

        public Usuario? Usuario { get; set; }
    }

    public enum EstadoCarrito
    {
        Activo = 0,
        Confirmado = 1,
        Cancelado = 2
    }
}
