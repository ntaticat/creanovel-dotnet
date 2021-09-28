using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CreaNovelNETCore.Migrations
{
    public partial class TerceraMigracion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tipo_recurso",
                table: "Recursos",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DecisionMensaje",
                table: "Recursos",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecursoDecisionOpciones",
                columns: table => new
                {
                    RecursoDecisionOpcionId = table.Column<Guid>(nullable: false),
                    OpcionMensaje = table.Column<string>(nullable: true),
                    SiguienteRecursoId = table.Column<Guid>(nullable: true),
                    RecursoDecisionId = table.Column<Guid>(nullable: false)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecursoDecisionOpciones");

            migrationBuilder.DropColumn(
                name: "tipo_recurso",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "DecisionMensaje",
                table: "Recursos");
        }
    }
}
