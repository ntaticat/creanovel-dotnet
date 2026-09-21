using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UnifyRecursoPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Contenido, SiguienteRecursoId, their index and the Recursos->Recursos self-FK were
            // already added (and backfilled) by the AddUnifiedRecursoColumns expand migration; this
            // migration only performs the destructive contract half now that the backfill is verified.
            migrationBuilder.DropForeignKey(
                name: "FK_RecursoDecisionOpciones_RecursosDecision_RecursoDecisionId",
                table: "RecursoDecisionOpciones");

            migrationBuilder.DropTable(
                name: "RecursosConversacion");

            migrationBuilder.DropTable(
                name: "RecursosDecision");

            migrationBuilder.DropTable(
                name: "RecursosEntrada");

            migrationBuilder.AddForeignKey(
                name: "FK_RecursoDecisionOpciones_Recursos_RecursoDecisionId",
                table: "RecursoDecisionOpciones",
                column: "RecursoDecisionId",
                principalTable: "Recursos",
                principalColumn: "RecursoId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecursoDecisionOpciones_Recursos_RecursoDecisionId",
                table: "RecursoDecisionOpciones");

            migrationBuilder.CreateTable(
                name: "RecursosConversacion",
                columns: table => new
                {
                    RecursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    SiguienteRecursoId = table.Column<Guid>(type: "uuid", nullable: true),
                    AutorMensaje = table.Column<string>(type: "text", nullable: true),
                    Mensaje = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosConversacion", x => x.RecursoId);
                    table.ForeignKey(
                        name: "FK_RecursosConversacion_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecursosConversacion_Recursos_SiguienteRecursoId",
                        column: x => x.SiguienteRecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RecursosDecision",
                columns: table => new
                {
                    RecursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutorDecisionMensaje = table.Column<string>(type: "text", nullable: true),
                    DecisionMensaje = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosDecision", x => x.RecursoId);
                    table.ForeignKey(
                        name: "FK_RecursosDecision_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecursosEntrada",
                columns: table => new
                {
                    RecursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    mensajeAviso = table.Column<string>(type: "text", nullable: true),
                    nombreVariable = table.Column<string>(type: "text", nullable: true),
                    valorVariable = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosEntrada", x => x.RecursoId);
                    table.ForeignKey(
                        name: "FK_RecursosEntrada_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Cascade);
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
    }
}
