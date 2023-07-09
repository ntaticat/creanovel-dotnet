using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class M6_AgregarEntidadRecursoEntrada : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecursosEntrada",
                columns: table => new
                {
                    RecursoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    mensajeAviso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nombreVariable = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    valorVariable = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosEntrada", x => x.RecursoId);
                    table.ForeignKey(
                        name: "FK_RecursosEntrada_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId");
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecursosEntrada");
        }
    }
}
