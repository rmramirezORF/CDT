using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IdEquipoPadre = table.Column<int>(type: "int", nullable: true),
                    IdLider = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUsuarioCreacion = table.Column<int>(type: "int", nullable: false),
                    IdUsuarioModificacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipo_Equipo_IdEquipoPadre",
                        column: x => x.IdEquipoPadre,
                        principalTable: "Equipo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipo_Usuario_IdLider",
                        column: x => x.IdLider,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EquipoMiembro",
                columns: table => new
                {
                    IdEquipo = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    FechaAgregado = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipoMiembro", x => new { x.IdEquipo, x.IdUsuario });
                    table.ForeignKey(
                        name: "FK_EquipoMiembro_Equipo_IdEquipo",
                        column: x => x.IdEquipo,
                        principalTable: "Equipo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipoMiembro_Usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipo_IdEquipoPadre",
                table: "Equipo",
                column: "IdEquipoPadre");

            migrationBuilder.CreateIndex(
                name: "IX_Equipo_IdLider",
                table: "Equipo",
                column: "IdLider");

            migrationBuilder.CreateIndex(
                name: "IX_Equipo_Nombre",
                table: "Equipo",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_EquipoMiembro_IdUsuario",
                table: "EquipoMiembro",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipoMiembro");

            migrationBuilder.DropTable(
                name: "Equipo");
        }
    }
}
