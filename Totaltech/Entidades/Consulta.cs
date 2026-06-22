using System.ComponentModel.DataAnnotations;

namespace Totaltech.Entidades
{
    public class Consulta
    {
        [Key]
        public int IdConsulta { get; set; }

        public int? IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        public DateTime FechaConsulta { get; set; }

        public EstadoConsulta Estado { get; set; } = EstadoConsulta.Pendiente;

        public Usuario? Usuario { get; set; }
    }

    public enum EstadoConsulta
    {
        Pendiente = 0,
        Respondida = 1,
        Cerrada = 2
    }
}
