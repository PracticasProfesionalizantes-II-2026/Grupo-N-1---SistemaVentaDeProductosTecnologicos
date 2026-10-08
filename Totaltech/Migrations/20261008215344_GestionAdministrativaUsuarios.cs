using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Totaltech.Migrations
{
    /// <inheritdoc />
    public partial class GestionAdministrativaUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "VersionSesion",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "AuditoriaUsuarios",
                columns: table => new
                {
                    IdAuditoriaUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdActor = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FechaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CamposModificados = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RolAnterior = table.Column<int>(type: "int", nullable: true),
                    RolNuevo = table.Column<int>(type: "int", nullable: true),
                    ActivoAnterior = table.Column<bool>(type: "bit", nullable: true),
                    ActivoNuevo = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaUsuarios", x => x.IdAuditoriaUsuario);
                    table.ForeignKey(
                        name: "FK_AuditoriaUsuarios_Usuarios_IdActor",
                        column: x => x.IdActor,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditoriaUsuarios_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaUsuarios_IdActor",
                table: "AuditoriaUsuarios",
                column: "IdActor");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaUsuarios_IdUsuario",
                table: "AuditoriaUsuarios",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaUsuarios");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "VersionSesion",
                table: "Usuarios");
        }
    }
}
