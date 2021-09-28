using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CreaNovelNETCore.Migrations
{
    public partial class CuartaMigracion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecursoDecisionOpciones_Recursos_RecursoDecisionId",
                table: "RecursoDecisionOpciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Recursos_Recursos_SiguienteRecursoId",
                table: "Recursos");

            migrationBuilder.DropIndex(
                name: "IX_Recursos_SiguienteRecursoId",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "DecisionMensaje",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "Mensaje",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "SiguienteRecursoId",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "tipo_recurso",
                table: "Recursos");

            migrationBuilder.CreateTable(
                name: "RecursosConversacion",
                columns: table => new
                {
                    RecursoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiguienteRecursoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosConversacion", x => x.RecursoId);
                    table.ForeignKey(
                        name: "FK_RecursosConversacion_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecursosConversacion_Recursos_SiguienteRecursoId",
                        column: x => x.SiguienteRecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecursosDecision",
                columns: table => new
                {
                    RecursoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecisionMensaje = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosDecision", x => x.RecursoId);
                    table.ForeignKey(
                        name: "FK_RecursosDecision_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecursosConversacion_SiguienteRecursoId",
                table: "RecursosConversacion",
                column: "SiguienteRecursoId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecursoDecisionOpciones_RecursosDecision_RecursoDecisionId",
                table: "RecursoDecisionOpciones",
                column: "RecursoDecisionId",
                principalTable: "RecursosDecision",
                principalColumn: "RecursoId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecursoDecisionOpciones_RecursosDecision_RecursoDecisionId",
                table: "RecursoDecisionOpciones");

            migrationBuilder.DropTable(
                name: "RecursosConversacion");

            migrationBuilder.DropTable(
                name: "RecursosDecision");

            migrationBuilder.AddColumn<string>(
                name: "DecisionMensaje",
                table: "Recursos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mensaje",
                table: "Recursos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SiguienteRecursoId",
                table: "Recursos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_recurso",
                table: "Recursos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_SiguienteRecursoId",
                table: "Recursos",
                column: "SiguienteRecursoId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecursoDecisionOpciones_Recursos_RecursoDecisionId",
                table: "RecursoDecisionOpciones",
                column: "RecursoDecisionId",
                principalTable: "Recursos",
                principalColumn: "RecursoId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Recursos_Recursos_SiguienteRecursoId",
                table: "Recursos",
                column: "SiguienteRecursoId",
                principalTable: "Recursos",
                principalColumn: "RecursoId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
