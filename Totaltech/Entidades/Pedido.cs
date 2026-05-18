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
    public class Pedido
    {
      
        
        [Key]
        public int IdPedido { get; set; }
        
        
        public int? IdUsuario { get; set; }

        /// Fecha y hora en que se generó el pedido.
        [DataType(DataType.DateTime)]
        public DateTime FechaPedido { get; set; }

        
        [Required]
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;


      
        [Required]
        public int IdDireccion { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario? Usuario { get; set; }

      
        /// Dirección vinculada al pedido.
      
        [ForeignKey(nameof(IdDireccion))]
        public Direccion? Direccion { get; set; }

      
        /// Prefactura generada a partir del pedido.
     
        public Prefacturacion? Prefacturacion { get; set; }
    }


    public enum EstadoPedido
    {
        /// Pedido creado pero aún no abonado.
        Pendiente = 0,

        /// Pedido pagado y listo para su preparación.
        Pagado = 1,

        /// Pedido despachado hacia el cliente.
        Enviado = 2,

        /// Pedido entregado satisfactoriamente.
        Entregado = 3,

        /// Pedido cancelado por el usuario o el comercio.
        Cancelado = 4
    }
}