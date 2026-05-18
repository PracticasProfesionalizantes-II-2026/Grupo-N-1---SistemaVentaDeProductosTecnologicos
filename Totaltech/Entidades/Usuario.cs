
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Totaltech.Entidades
{
 
    public class Usuario
    {
     
        [Key]
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        public DateTime FechaRegistro { get; set; }

   
        public RolUsuario Rol { get; set; } = RolUsuario.Cliente;


     
    }

    public enum RolUsuario
    {

        Cliente = 0,

        Administrador = 1
    }
}