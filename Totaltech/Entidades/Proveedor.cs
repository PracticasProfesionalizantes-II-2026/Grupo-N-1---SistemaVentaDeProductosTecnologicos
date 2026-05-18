using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{

    public class Proveedor
    {

        [Key]
        public int IdProveedor { get; set; }

        public string RazonSocial { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Cuit { get; set; } = string.Empty;


        [Required]
        public string EmailComercial { get; set; } = string.Empty;

        [Phone]
        public string TelefonoComercial { get; set; } = string.Empty;


        [Required]
        public string CondicionIva { get; set; } = string.Empty;

        [Required]
        public string DireccionFiscal { get; set; } = string.Empty;

        public int PlazoPagoDias { get; set; }

 
        public int TiempoEntregaDias { get; set; }

        [Required]
        public string MonedaPreferida { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;


    }
}