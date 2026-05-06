using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTareasYClaveProyecto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Clave",
                table: "Proyecto",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UltimoNumeroTarea",
                table: "Proyecto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TipoActividad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Icono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoActividad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tarea",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroEnProyecto = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IdProyecto = table.Column<int>(type: "int", nullable: false),
                    IdLista = table.Column<int>(type: "int", nullable: false),
                    IdTipoActividad = table.Column<int>(type: "int", nullable: true),
                    IdEstado = table.Column<int>(type: "int", nullable: true),
                    IdPrioridad = table.Column<int>(type: "int", nullable: true),
                    IdResponsable = table.Column<int>(type: "int", nullable: true),
                    IdInformador = table.Column<int>(type: "int", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUsuarioCreacion = table.Column<int>(type: "int", nullable: false),
                    IdUsuarioModificacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarea", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tarea_Estado_IdEstado",
                        column: x => x.IdEstado,
                        principalTable: "Estado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tarea_Lista_IdLista",
                        column: x => x.IdLista,
                        principalTable: "Lista",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tarea_Prioridad_IdPrioridad",
                        column: x => x.IdPrioridad,
                        principalTable: "Prioridad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tarea_Proyecto_IdProyecto",
                        column: x => x.IdProyecto,
                        principalTable: "Proyecto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarea_TipoActividad_IdTipoActividad",
                        column: x => x.IdTipoActividad,
                        principalTable: "TipoActividad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tarea_Usuario_IdInformador",
                        column: x => x.IdInformador,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tarea_Usuario_IdResponsable",
                        column: x => x.IdResponsable,
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proyecto_Clave",
                table: "Proyecto",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_IdEstado",
                table: "Tarea",
                column: "IdEstado");

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_IdInformador",
                table: "Tarea",
                column: "IdInformador");

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_IdLista",
                table: "Tarea",
                column: "IdLista");

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_IdLista_Orden",
                table: "Tarea",
                columns: new[] { "IdLista", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_IdPrioridad",
                table: "Tarea",
                column: "IdPrioridad");

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_IdProyecto",
                table: "Tarea",
                column: "IdProyecto");

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_IdProyecto_NumeroEnProyecto",
                table: "Tarea",
                columns: new[] { "IdProyecto", "NumeroEnProyecto" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_IdResponsable",
                table: "Tarea",
                column: "IdResponsable");

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_IdTipoActividad",
                table: "Tarea",
                column: "IdTipoActividad");

            migrationBuilder.CreateIndex(
                name: "IX_TipoActividad_Nombre",
                table: "TipoActividad",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tarea");

            migrationBuilder.DropTable(
                name: "TipoActividad");

            migrationBuilder.DropIndex(
                name: "IX_Proyecto_Clave",
                table: "Proyecto");

            migrationBuilder.DropColumn(
                name: "Clave",
                table: "Proyecto");

            migrationBuilder.DropColumn(
                name: "UltimoNumeroTarea",
                table: "Proyecto");
        }
    }
}
