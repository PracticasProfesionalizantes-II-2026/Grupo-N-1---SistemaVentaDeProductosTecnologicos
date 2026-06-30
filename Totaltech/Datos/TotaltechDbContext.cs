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

        public DbSet<DetalleCarrito> DetalleCarritos { get; set; }

        public DbSet<Pago> Pagos { get; set; }

        public DbSet<Compra> Compras { get; set; }

        public DbSet<Reporte> Reportes { get; set; }

        public DbSet<Consulta> Consultas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .Property(usuario => usuario.Email)
                .HasMaxLength(256)
                .IsRequired();

            modelBuilder.Entity<Usuario>()
                .Property(usuario => usuario.Contrasena)
                .HasMaxLength(500)
                .IsRequired();

            modelBuilder.Entity<Usuario>()
                .HasIndex(usuario => usuario.Email)
                .IsUnique();

            modelBuilder.Entity<DetalleCarrito>()
                .HasIndex(detalle => new { detalle.IdCarrito, detalle.IdProducto })
                .IsUnique();

            modelBuilder.Entity<Carrito>()
                .HasOne(carrito => carrito.Usuario)
                .WithMany()
                .HasForeignKey(carrito => carrito.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleCarrito>()
                .HasOne(detalle => detalle.Carrito)
                .WithMany()
                .HasForeignKey(detalle => detalle.IdCarrito)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleCarrito>()
                .HasOne(detalle => detalle.Producto)
                .WithMany()
                .HasForeignKey(detalle => detalle.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compra>()
                .HasOne(compra => compra.Proveedor)
                .WithMany()
                .HasForeignKey(compra => compra.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetallePedido>()
                .HasOne(detalle => detalle.Pedido)
                .WithMany()
                .HasForeignKey(detalle => detalle.IdPedido)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetallePedido>()
                .HasOne(detalle => detalle.Producto)
                .WithMany()
                .HasForeignKey(detalle => detalle.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Direccion>()
                .HasOne(direccion => direccion.Usuario)
                .WithMany()
                .HasForeignKey(direccion => direccion.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pago>()
                .HasOne(pago => pago.Pedido)
                .WithMany()
                .HasForeignKey(pago => pago.IdPedido)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pedido>()
                .HasOne(pedido => pedido.Usuario)
                .WithMany()
                .HasForeignKey(pedido => pedido.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pedido>()
                .HasOne(pedido => pedido.Direccion)
                .WithMany()
                .HasForeignKey(pedido => pedido.IdDireccion)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Producto>()
                .HasOne(producto => producto.Categoria)
                .WithMany()
                .HasForeignKey(producto => producto.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Producto>()
                .HasOne(producto => producto.Proveedor)
                .WithMany()
                .HasForeignKey(producto => producto.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Proveedor>()
                .HasOne(proveedor => proveedor.Direccion)
                .WithMany()
                .HasForeignKey(proveedor => proveedor.IdDireccion)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reporte>()
                .HasOne(reporte => reporte.Usuario)
                .WithMany()
                .HasForeignKey(reporte => reporte.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consulta>()
                .HasOne(consulta => consulta.Usuario)
                .WithMany()
                .HasForeignKey(consulta => consulta.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
