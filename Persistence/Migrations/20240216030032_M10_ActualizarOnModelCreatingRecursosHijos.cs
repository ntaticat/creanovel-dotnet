using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class M10_ActualizarOnModelCreatingRecursosHijos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecursosConversacion_Recursos_SiguienteRecursoId",
                table: "RecursosConversacion");

            migrationBuilder.AddForeignKey(
                name: "FK_RecursosConversacion_Recursos_SiguienteRecursoId",
                table: "RecursosConversacion",
                column: "SiguienteRecursoId",
                principalTable: "Recursos",
                principalColumn: "RecursoId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecursosConversacion_Recursos_SiguienteRecursoId",
                table: "RecursosConversacion");

            migrationBuilder.AddForeignKey(
                name: "FK_RecursosConversacion_Recursos_SiguienteRecursoId",
                table: "RecursosConversacion",
                column: "SiguienteRecursoId",
                principalTable: "Recursos",
                principalColumn: "RecursoId");
        }
    }
}
