using System.ComponentModel.DataAnnotations;

namespace Totaltech.Entidades
{
    public class Categoria
    {
        [Key]
        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;
    }
}
