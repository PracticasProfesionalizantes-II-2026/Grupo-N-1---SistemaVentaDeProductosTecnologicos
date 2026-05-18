using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    /// <summary>
    /// Representa una categoría de productos.
    /// Clase mínima para las relaciones con Producto y Promoción.
    /// </summary>
    public class Categoria
    {
        [Key]
        public int IdCategoria { get; set; }

        public string? Nombre { get; set; } 

        public string? Descripcion { get; set; }

       
    }
}
