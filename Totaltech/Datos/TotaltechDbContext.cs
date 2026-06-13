using Microsoft.EntityFrameworkCore;
using Totaltech.Entidades;

namespace Totaltech.Datos
{
    public class TotaltechDbContext : DbContext
    {
        public TotaltechDbContext(DbContextOptions<TotaltechDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Direccion> Direcciones { get; set; }

        public DbSet<Proveedor> Proveedores { get; set; }

        public DbSet<Producto> Productos { get; set; }

        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<Pedido> Pedidos { get; set; }

        public DbSet<DetallePedido> DetallePedidos { get; set; }

        public DbSet<Carrito> Carritos { get; set; }

        public DbSet<Pago> Pagos { get; set; }

        public DbSet<Compra> Compras { get; set; }

        public DbSet<Reporte> Reportes { get; set; }
    }
}
