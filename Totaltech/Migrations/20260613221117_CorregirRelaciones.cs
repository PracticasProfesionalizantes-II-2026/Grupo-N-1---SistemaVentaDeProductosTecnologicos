using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Totaltech.Migrations
{
    /// <inheritdoc />
    public partial class CorregirRelaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carritos_Usuarios_UsuarioIdUsuario",
                table: "Carritos");

            migrationBuilder.DropForeignKey(
                name: "FK_Compras_Proveedores_ProveedorIdProveedor",
                table: "Compras");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallePedidos_Pedidos_PedidoIdPedido",
                table: "DetallePedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallePedidos_Productos_ProductoIdProducto",
                table: "DetallePedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Direcciones_Usuarios_IdUsuario",
                table: "Direcciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Pedidos_PedidoIdPedido",
                table: "Pagos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Direcciones_DireccionIdDireccion",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Usuarios_UsuarioIdUsuario",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Categorias_CategoriaIdCategoria",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Proveedores_ProveedorIdProveedor",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Proveedores_Direcciones_DireccionIdDireccion",
                table: "Proveedores");

            migrationBuilder.DropForeignKey(
                name: "FK_Reportes_Usuarios_UsuarioIdUsuario",
                table: "Reportes");

            migrationBuilder.DropIndex(
                name: "IX_Reportes_UsuarioIdUsuario",
                table: "Reportes");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_DireccionIdDireccion",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Productos_CategoriaIdCategoria",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_ProveedorIdProveedor",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_DireccionIdDireccion",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_UsuarioIdUsuario",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_PedidoIdPedido",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_DetallePedidos_PedidoIdPedido",
                table: "DetallePedidos");

            migrationBuilder.DropIndex(
                name: "IX_DetallePedidos_ProductoIdProducto",
                table: "DetallePedidos");

            migrationBuilder.DropIndex(
                name: "IX_Compras_ProveedorIdProveedor",
                table: "Compras");

            migrationBuilder.DropIndex(
                name: "IX_Carritos_UsuarioIdUsuario",
                table: "Carritos");

            migrationBuilder.DropColumn(
                name: "UsuarioIdUsuario",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "DireccionIdDireccion",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "CategoriaIdCategoria",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "ProveedorIdProveedor",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "DireccionIdDireccion",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "UsuarioIdUsuario",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "PedidoIdPedido",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "PedidoIdPedido",
                table: "DetallePedidos");

            migrationBuilder.DropColumn(
                name: "ProductoIdProducto",
                table: "DetallePedidos");

            migrationBuilder.DropColumn(
                name: "ProveedorIdProveedor",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "UsuarioIdUsuario",
                table: "Carritos");

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_IdUsuario",
                table: "Reportes",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_IdDireccion",
                table: "Proveedores",
                column: "IdDireccion");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IdCategoria",
                table: "Productos",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IdProveedor",
                table: "Productos",
                column: "IdProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdDireccion",
                table: "Pedidos",
                column: "IdDireccion");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdUsuario",
                table: "Pedidos",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_IdPedido",
                table: "Pagos",
                column: "IdPedido");

            migrationBuilder.CreateIndex(
                name: "IX_DetallePedidos_IdPedido",
                table: "DetallePedidos",
                column: "IdPedido");

            migrationBuilder.CreateIndex(
                name: "IX_DetallePedidos_IdProducto",
                table: "DetallePedidos",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_IdProveedor",
                table: "Compras",
                column: "IdProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Carritos_IdUsuario",
                table: "Carritos",
                column: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Carritos_Usuarios_IdUsuario",
                table: "Carritos",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_Proveedores_IdProveedor",
                table: "Compras",
                column: "IdProveedor",
                principalTable: "Proveedores",
                principalColumn: "IdProveedor",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallePedidos_Pedidos_IdPedido",
                table: "DetallePedidos",
                column: "IdPedido",
                principalTable: "Pedidos",
                principalColumn: "IdPedido",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallePedidos_Productos_IdProducto",
                table: "DetallePedidos",
                column: "IdProducto",
                principalTable: "Productos",
                principalColumn: "IdProducto",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Direcciones_Usuarios_IdUsuario",
                table: "Direcciones",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Pedidos_IdPedido",
                table: "Pagos",
                column: "IdPedido",
                principalTable: "Pedidos",
                principalColumn: "IdPedido",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Direcciones_IdDireccion",
                table: "Pedidos",
                column: "IdDireccion",
                principalTable: "Direcciones",
                principalColumn: "IdDireccion",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Usuarios_IdUsuario",
                table: "Pedidos",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Categorias_IdCategoria",
                table: "Productos",
                column: "IdCategoria",
                principalTable: "Categorias",
                principalColumn: "IdCategoria",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Proveedores_IdProveedor",
                table: "Productos",
                column: "IdProveedor",
                principalTable: "Proveedores",
                principalColumn: "IdProveedor",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Proveedores_Direcciones_IdDireccion",
                table: "Proveedores",
                column: "IdDireccion",
                principalTable: "Direcciones",
                principalColumn: "IdDireccion",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reportes_Usuarios_IdUsuario",
                table: "Reportes",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carritos_Usuarios_IdUsuario",
                table: "Carritos");

            migrationBuilder.DropForeignKey(
                name: "FK_Compras_Proveedores_IdProveedor",
                table: "Compras");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallePedidos_Pedidos_IdPedido",
                table: "DetallePedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallePedidos_Productos_IdProducto",
                table: "DetallePedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Direcciones_Usuarios_IdUsuario",
                table: "Direcciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Pedidos_IdPedido",
                table: "Pagos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Direcciones_IdDireccion",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Usuarios_IdUsuario",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Categorias_IdCategoria",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Proveedores_IdProveedor",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Proveedores_Direcciones_IdDireccion",
                table: "Proveedores");

            migrationBuilder.DropForeignKey(
                name: "FK_Reportes_Usuarios_IdUsuario",
                table: "Reportes");

            migrationBuilder.DropIndex(
                name: "IX_Reportes_IdUsuario",
                table: "Reportes");

            migrationBuilder.DropIndex(
                name: "IX_Proveedores_IdDireccion",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Productos_IdCategoria",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_IdProveedor",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_IdDireccion",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_IdUsuario",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_IdPedido",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_DetallePedidos_IdPedido",
                table: "DetallePedidos");

            migrationBuilder.DropIndex(
                name: "IX_DetallePedidos_IdProducto",
                table: "DetallePedidos");

            migrationBuilder.DropIndex(
                name: "IX_Compras_IdProveedor",
                table: "Compras");

            migrationBuilder.DropIndex(
                name: "IX_Carritos_IdUsuario",
                table: "Carritos");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioIdUsuario",
                table: "Reportes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DireccionIdDireccion",
                table: "Proveedores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoriaIdCategoria",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProveedorIdProveedor",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DireccionIdDireccion",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioIdUsuario",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PedidoIdPedido",
                table: "Pagos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PedidoIdPedido",
                table: "DetallePedidos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductoIdProducto",
                table: "DetallePedidos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProveedorIdProveedor",
                table: "Compras",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioIdUsuario",
                table: "Carritos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_UsuarioIdUsuario",
                table: "Reportes",
                column: "UsuarioIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_DireccionIdDireccion",
                table: "Proveedores",
                column: "DireccionIdDireccion");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaIdCategoria",
                table: "Productos",
                column: "CategoriaIdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_ProveedorIdProveedor",
                table: "Productos",
                column: "ProveedorIdProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_DireccionIdDireccion",
                table: "Pedidos",
                column: "DireccionIdDireccion");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_UsuarioIdUsuario",
                table: "Pedidos",
                column: "UsuarioIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_PedidoIdPedido",
                table: "Pagos",
                column: "PedidoIdPedido");

            migrationBuilder.CreateIndex(
                name: "IX_DetallePedidos_PedidoIdPedido",
                table: "DetallePedidos",
                column: "PedidoIdPedido");

            migrationBuilder.CreateIndex(
                name: "IX_DetallePedidos_ProductoIdProducto",
                table: "DetallePedidos",
                column: "ProductoIdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_ProveedorIdProveedor",
                table: "Compras",
                column: "ProveedorIdProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Carritos_UsuarioIdUsuario",
                table: "Carritos",
                column: "UsuarioIdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Carritos_Usuarios_UsuarioIdUsuario",
                table: "Carritos",
                column: "UsuarioIdUsuario",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_Proveedores_ProveedorIdProveedor",
                table: "Compras",
                column: "ProveedorIdProveedor",
                principalTable: "Proveedores",
                principalColumn: "IdProveedor");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallePedidos_Pedidos_PedidoIdPedido",
                table: "DetallePedidos",
                column: "PedidoIdPedido",
                principalTable: "Pedidos",
                principalColumn: "IdPedido");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallePedidos_Productos_ProductoIdProducto",
                table: "DetallePedidos",
                column: "ProductoIdProducto",
                principalTable: "Productos",
                principalColumn: "IdProducto");

            migrationBuilder.AddForeignKey(
                name: "FK_Direcciones_Usuarios_IdUsuario",
                table: "Direcciones",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Pedidos_PedidoIdPedido",
                table: "Pagos",
                column: "PedidoIdPedido",
                principalTable: "Pedidos",
                principalColumn: "IdPedido");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Direcciones_DireccionIdDireccion",
                table: "Pedidos",
                column: "DireccionIdDireccion",
                principalTable: "Direcciones",
                principalColumn: "IdDireccion");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Usuarios_UsuarioIdUsuario",
                table: "Pedidos",
                column: "UsuarioIdUsuario",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Categorias_CategoriaIdCategoria",
                table: "Productos",
                column: "CategoriaIdCategoria",
                principalTable: "Categorias",
                principalColumn: "IdCategoria");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Proveedores_ProveedorIdProveedor",
                table: "Productos",
                column: "ProveedorIdProveedor",
                principalTable: "Proveedores",
                principalColumn: "IdProveedor");

            migrationBuilder.AddForeignKey(
                name: "FK_Proveedores_Direcciones_DireccionIdDireccion",
                table: "Proveedores",
                column: "DireccionIdDireccion",
                principalTable: "Direcciones",
                principalColumn: "IdDireccion");

            migrationBuilder.AddForeignKey(
                name: "FK_Reportes_Usuarios_UsuarioIdUsuario",
                table: "Reportes",
                column: "UsuarioIdUsuario",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario");
        }
    }
}
