using Microsoft.EntityFrameworkCore.Migrations;

namespace CreaNovelNETCore.Migrations
{
    public partial class QuintaMigracion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecursoDecisionOpciones_Recursos_SiguienteRecursoId",
                table: "RecursoDecisionOpciones");

            migrationBuilder.DropForeignKey(
                name: "FK_RecursoDecisionOpciones_RecursosDecision_RecursoDecisionId",
                table: "RecursoDecisionOpciones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecursoDecisionOpciones",
                table: "RecursoDecisionOpciones");

            migrationBuilder.RenameTable(
                name: "RecursoDecisionOpciones",
                newName: "RecursoDecisionOpcion");

            migrationBuilder.RenameIndex(
                name: "IX_RecursoDecisionOpciones_SiguienteRecursoId",
                table: "RecursoDecisionOpcion",
                newName: "IX_RecursoDecisionOpcion_SiguienteRecursoId");

            migrationBuilder.RenameIndex(
                name: "IX_RecursoDecisionOpciones_RecursoDecisionId",
                table: "RecursoDecisionOpcion",
                newName: "IX_RecursoDecisionOpcion_RecursoDecisionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecursoDecisionOpcion",
                table: "RecursoDecisionOpcion",
                column: "RecursoDecisionOpcionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecursoDecisionOpcion_Recursos_SiguienteRecursoId",
                table: "RecursoDecisionOpcion",
                column: "SiguienteRecursoId",
                principalTable: "Recursos",
                principalColumn: "RecursoId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecursoDecisionOpcion_RecursosDecision_RecursoDecisionId",
                table: "RecursoDecisionOpcion",
                column: "RecursoDecisionId",
                principalTable: "RecursosDecision",
                principalColumn: "RecursoId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecursoDecisionOpcion_Recursos_SiguienteRecursoId",
                table: "RecursoDecisionOpcion");

            migrationBuilder.DropForeignKey(
                name: "FK_RecursoDecisionOpcion_RecursosDecision_RecursoDecisionId",
                table: "RecursoDecisionOpcion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecursoDecisionOpcion",
                table: "RecursoDecisionOpcion");

            migrationBuilder.RenameTable(
                name: "RecursoDecisionOpcion",
                newName: "RecursoDecisionOpciones");

            migrationBuilder.RenameIndex(
                name: "IX_RecursoDecisionOpcion_SiguienteRecursoId",
                table: "RecursoDecisionOpciones",
                newName: "IX_RecursoDecisionOpciones_SiguienteRecursoId");

            migrationBuilder.RenameIndex(
                name: "IX_RecursoDecisionOpcion_RecursoDecisionId",
                table: "RecursoDecisionOpciones",
                newName: "IX_RecursoDecisionOpciones_RecursoDecisionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecursoDecisionOpciones",
                table: "RecursoDecisionOpciones",
                column: "RecursoDecisionOpcionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecursoDecisionOpciones_Recursos_SiguienteRecursoId",
                table: "RecursoDecisionOpciones",
                column: "SiguienteRecursoId",
                principalTable: "Recursos",
                principalColumn: "RecursoId",
                onDelete: ReferentialAction.Restrict);

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
