using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUnifiedRecursoColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SiguienteRecursoId",
                table: "Recursos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contenido",
                table: "Recursos",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.Sql(@"
                UPDATE ""Recursos"" r
                SET ""SiguienteRecursoId"" = rc.""SiguienteRecursoId"",
                    ""Contenido"" = jsonb_build_object('Mensaje', rc.""Mensaje"", 'AutorMensaje', rc.""AutorMensaje"")
                FROM ""RecursosConversacion"" rc WHERE rc.""RecursoId"" = r.""RecursoId"";
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Recursos"" r
                SET ""Contenido"" = jsonb_build_object('DecisionMensaje', rd.""DecisionMensaje"", 'AutorDecisionMensaje', rd.""AutorDecisionMensaje"")
                FROM ""RecursosDecision"" rd WHERE rd.""RecursoId"" = r.""RecursoId"";
            ");

            migrationBuilder.Sql(@"
                DO $$ DECLARE missing int; BEGIN
                    SELECT COUNT(*) INTO missing FROM ""Recursos""
                    WHERE ""TipoRecurso"" IN ('recurso_conversacion','recurso_decision') AND ""Contenido"" = '{}'::jsonb;
                    IF missing > 0 THEN RAISE EXCEPTION 'Backfill incompleto: % filas', missing; END IF;
                END $$;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_SiguienteRecursoId",
                table: "Recursos",
                column: "SiguienteRecursoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recursos_Recursos_SiguienteRecursoId",
                table: "Recursos",
                column: "SiguienteRecursoId",
                principalTable: "Recursos",
                principalColumn: "RecursoId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recursos_Recursos_SiguienteRecursoId",
                table: "Recursos");

            migrationBuilder.DropIndex(
                name: "IX_Recursos_SiguienteRecursoId",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "SiguienteRecursoId",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "Contenido",
                table: "Recursos");
        }
    }
}
