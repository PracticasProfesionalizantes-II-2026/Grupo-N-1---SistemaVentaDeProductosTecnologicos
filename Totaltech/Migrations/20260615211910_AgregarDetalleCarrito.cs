using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Totaltech.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDetalleCarrito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DetalleCarritos",
                columns: table => new
                {
                    IdDetalleCarrito = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCarrito = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleCarritos", x => x.IdDetalleCarrito);
                    table.ForeignKey(
                        name: "FK_DetalleCarritos_Carritos_IdCarrito",
                        column: x => x.IdCarrito,
                        principalTable: "Carritos",
                        principalColumn: "IdCarrito",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleCarritos_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleCarritos_IdCarrito",
                table: "DetalleCarritos",
                column: "IdCarrito");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleCarritos_IdProducto",
                table: "DetalleCarritos",
                column: "IdProducto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleCarritos");
        }
    }
}
