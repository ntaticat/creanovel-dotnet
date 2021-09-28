using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CreaNovelNETCore.Migrations
{
    public partial class SegundaMigracion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecursoDecisionOpciones");

            migrationBuilder.DropColumn(
                name: "tipo_recurso",
                table: "Recursos");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tipo_recurso",
                table: "Recursos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "RecursoDecisionOpciones",
                columns: table => new
                {
                    RecursoDecisionOpcionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecursoDecisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiguienteRecursoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursoDecisionOpciones", x => x.RecursoDecisionOpcionId);
                    table.ForeignKey(
                        name: "FK_RecursoDecisionOpciones_Recursos_RecursoDecisionId",
                        column: x => x.RecursoDecisionId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecursoDecisionOpciones_Recursos_SiguienteRecursoId",
                        column: x => x.SiguienteRecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecursoDecisionOpciones_RecursoDecisionId",
                table: "RecursoDecisionOpciones",
                column: "RecursoDecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecursoDecisionOpciones_SiguienteRecursoId",
                table: "RecursoDecisionOpciones",
                column: "SiguienteRecursoId");
        }
    }
}
