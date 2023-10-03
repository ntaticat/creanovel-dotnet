using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class M8_RegistrarNovelaVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Escenas_Novelas_NovelaId",
                table: "Escenas");

            migrationBuilder.RenameColumn(
                name: "NovelaId",
                table: "Escenas",
                newName: "NovelaVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_Escenas_NovelaId",
                table: "Escenas",
                newName: "IX_Escenas_NovelaVersionId");

            migrationBuilder.CreateTable(
                name: "NovelaVersiones",
                columns: table => new
                {
                    NovelaVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NovelaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NovelaVersiones", x => x.NovelaVersionId);
                    table.ForeignKey(
                        name: "FK_NovelaVersiones_Novelas_NovelaId",
                        column: x => x.NovelaId,
                        principalTable: "Novelas",
                        principalColumn: "NovelaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NovelaVersiones_NovelaId",
                table: "NovelaVersiones",
                column: "NovelaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Escenas_NovelaVersiones_NovelaVersionId",
                table: "Escenas",
                column: "NovelaVersionId",
                principalTable: "NovelaVersiones",
                principalColumn: "NovelaVersionId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Escenas_NovelaVersiones_NovelaVersionId",
                table: "Escenas");

            migrationBuilder.DropTable(
                name: "NovelaVersiones");

            migrationBuilder.RenameColumn(
                name: "NovelaVersionId",
                table: "Escenas",
                newName: "NovelaId");

            migrationBuilder.RenameIndex(
                name: "IX_Escenas_NovelaVersionId",
                table: "Escenas",
                newName: "IX_Escenas_NovelaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Escenas_Novelas_NovelaId",
                table: "Escenas",
                column: "NovelaId",
                principalTable: "Novelas",
                principalColumn: "NovelaId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
