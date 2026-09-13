using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Totaltech.Migrations
{
    /// <inheritdoc />
    public partial class ProtegerCheckoutEtapa2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DireccionCalle",
                table: "Pedidos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DireccionCiudad",
                table: "Pedidos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DireccionCodigoPostal",
                table: "Pedidos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DireccionNumero",
                table: "Pedidos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DireccionPais",
                table: "Pedidos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DireccionProvincia",
                table: "Pedidos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IdCarrito",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Pedidos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE pedido
                SET pedido.Total = COALESCE(detalles.Total, 0),
                    pedido.DireccionCalle = COALESCE(direccion.Calle, ''),
                    pedido.DireccionNumero = COALESCE(direccion.Numero, ''),
                    pedido.DireccionCiudad = COALESCE(direccion.Ciudad, ''),
                    pedido.DireccionProvincia = COALESCE(direccion.Provincia, ''),
                    pedido.DireccionCodigoPostal = COALESCE(direccion.CodigoPostal, ''),
                    pedido.DireccionPais = COALESCE(direccion.Pais, '')
                FROM Pedidos AS pedido
                LEFT JOIN Direcciones AS direccion
                    ON direccion.IdDireccion = pedido.IdDireccion
                OUTER APPLY
                (
                    SELECT SUM(detalle.Subtotal) AS Total
                    FROM DetallePedidos AS detalle
                    WHERE detalle.IdPedido = pedido.IdPedido
                ) AS detalles;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Stock_NoNegativo",
                table: "Productos",
                sql: "[Stock] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdCarrito",
                table: "Pedidos",
                column: "IdCarrito",
                unique: true,
                filter: "[IdCarrito] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Pedidos_Total_NoNegativo",
                table: "Pedidos",
                sql: "[Total] >= 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Carritos_IdCarrito",
                table: "Pedidos",
                column: "IdCarrito",
                principalTable: "Carritos",
                principalColumn: "IdCarrito",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Carritos_IdCarrito",
                table: "Pedidos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Stock_NoNegativo",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_IdCarrito",
                table: "Pedidos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Pedidos_Total_NoNegativo",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DireccionCalle",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DireccionCiudad",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DireccionCodigoPostal",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DireccionNumero",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DireccionPais",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "DireccionProvincia",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "IdCarrito",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Pedidos");
        }
    }
}
