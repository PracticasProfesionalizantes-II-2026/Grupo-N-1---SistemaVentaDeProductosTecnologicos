using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    /// <summary>
    /// Representa una dirección asociada a un usuario para envíos o facturación.
    /// </summary>
    public class Direccion
    {
  
        [Key]
        public int IdDireccion { get; set; }
        public int? IdUsuario { get; set; }
        [ForeignKey(nameof(IdUsuario))]
        public Usuario? Usuario { get; set; }
        public string Calle { get; set; } 
        public string Numero { get; set; } 
        public string Ciudad { get; set; } 
        public string Provincia { get; set; } 
        public string CodigoPostal { get; set; } 
        public string Pais { get; set; } 
        public TipoDireccion Tipo { get; set; } = TipoDireccion.Envio;


    }


    public enum TipoDireccion
    {

        Envio = 0,

        Facturacion = 1,

        Fiscal = 2
    }
}