using System.ComponentModel.DataAnnotations;

namespace Totaltech.Entidades
{
    public class Reporte
    {
        [Key]
        public int IdReporte { get; set; }

        [Required]
        public TipoReporte TipoReporte { get; set; } = TipoReporte.Ventas;

        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }

        public int IdUsuario { get; set; }

        public Usuario? Usuario { get; set; }
    }

    public enum TipoReporte
    {
        Ventas = 0,
        Compras = 1,
        Usuarios = 2
    }
}
