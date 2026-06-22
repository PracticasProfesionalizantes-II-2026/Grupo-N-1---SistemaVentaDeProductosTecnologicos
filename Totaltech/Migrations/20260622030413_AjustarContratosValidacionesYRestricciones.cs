using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Totaltech.Migrations
{
    /// <inheritdoc />
    public partial class AjustarContratosValidacionesYRestricciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Usuarios
                SET Email = CONCAT('usuario', IdUsuario, '@sin-email.local')
                WHERE Email IS NULL OR LTRIM(RTRIM(Email)) = '';

                WITH EmailsDuplicados AS (
                    SELECT IdUsuario,
                           ROW_NUMBER() OVER (PARTITION BY Email ORDER BY IdUsuario) AS Fila
                    FROM Usuarios
                )
                UPDATE Usuarios
                SET Email = CONCAT('usuario', Usuarios.IdUsuario, '@duplicado.local')
                FROM Usuarios
                INNER JOIN EmailsDuplicados ON Usuarios.IdUsuario = EmailsDuplicados.IdUsuario
                WHERE EmailsDuplicados.Fila > 1;

                WITH DetallesAgrupados AS (
                    SELECT IdCarrito,
                           IdProducto,
                           MIN(IdDetalleCarrito) AS IdDetalleCarritoPrincipal,
                           SUM(Cantidad) AS CantidadTotal,
                           SUM(Subtotal) AS SubtotalTotal,
                           COUNT(*) AS CantidadRegistros
                    FROM DetalleCarritos
                    GROUP BY IdCarrito, IdProducto
                )
                UPDATE DetalleCarritos
                SET Cantidad = DetallesAgrupados.CantidadTotal,
                    Subtotal = DetallesAgrupados.SubtotalTotal,
                    PrecioUnitario = CASE
                        WHEN DetallesAgrupados.CantidadTotal > 0
                            THEN DetallesAgrupados.SubtotalTotal / DetallesAgrupados.CantidadTotal
                        ELSE DetalleCarritos.PrecioUnitario
                    END
                FROM DetalleCarritos
                INNER JOIN DetallesAgrupados ON DetalleCarritos.IdDetalleCarrito = DetallesAgrupados.IdDetalleCarritoPrincipal
                WHERE DetallesAgrupados.CantidadRegistros > 1;

                WITH DetallesAgrupados AS (
                    SELECT IdCarrito,
                           IdProducto,
                           MIN(IdDetalleCarrito) AS IdDetalleCarritoPrincipal,
                           COUNT(*) AS CantidadRegistros
                    FROM DetalleCarritos
                    GROUP BY IdCarrito, IdProducto
                )
                DELETE DetalleCarritos
                FROM DetalleCarritos
                INNER JOIN DetallesAgrupados
                    ON DetalleCarritos.IdCarrito = DetallesAgrupados.IdCarrito
                    AND DetalleCarritos.IdProducto = DetallesAgrupados.IdProducto
                WHERE DetallesAgrupados.CantidadRegistros > 1
                    AND DetalleCarritos.IdDetalleCarrito <> DetallesAgrupados.IdDetalleCarritoPrincipal;
                """);

            migrationBuilder.DropIndex(
                name: "IX_DetalleCarritos_IdCarrito",
                table: "DetalleCarritos");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Contrasena",
                table: "Usuarios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetalleCarritos_IdCarrito_IdProducto",
                table: "DetalleCarritos",
                columns: new[] { "IdCarrito", "IdProducto" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_DetalleCarritos_IdCarrito_IdProducto",
                table: "DetalleCarritos");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Contrasena",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_DetalleCarritos_IdCarrito",
                table: "DetalleCarritos",
                column: "IdCarrito");
        }
    }
}
