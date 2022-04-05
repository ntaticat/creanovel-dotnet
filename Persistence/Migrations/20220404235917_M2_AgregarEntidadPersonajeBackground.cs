using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class M2_AgregarEntidadPersonajeBackground : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BackgroundSpriteId",
                table: "RecursosDecision",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PersonajeSpriteId",
                table: "RecursosDecision",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutorMensaje",
                table: "RecursosConversacion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BackgroundSpriteId",
                table: "RecursosConversacion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PersonajeSpriteId",
                table: "RecursosConversacion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Backgrounds",
                columns: table => new
                {
                    BackgroundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backgrounds", x => x.BackgroundId);
                });

            migrationBuilder.CreateTable(
                name: "Personajes",
                columns: table => new
                {
                    PersonajeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personajes", x => x.PersonajeId);
                });

            migrationBuilder.CreateTable(
                name: "BackgroundSprites",
                columns: table => new
                {
                    BackgroundSpriteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DireccionImagen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BackgroundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundSprites", x => x.BackgroundSpriteId);
                    table.ForeignKey(
                        name: "FK_BackgroundSprites_Backgrounds_BackgroundId",
                        column: x => x.BackgroundId,
                        principalTable: "Backgrounds",
                        principalColumn: "BackgroundId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NovelaBackground",
                columns: table => new
                {
                    NovelaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BackgroundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NovelaBackground", x => new { x.NovelaId, x.BackgroundId });
                    table.ForeignKey(
                        name: "FK_NovelaBackground_Backgrounds_BackgroundId",
                        column: x => x.BackgroundId,
                        principalTable: "Backgrounds",
                        principalColumn: "BackgroundId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NovelaBackground_Novelas_NovelaId",
                        column: x => x.NovelaId,
                        principalTable: "Novelas",
                        principalColumn: "NovelaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NovelaPersonaje",
                columns: table => new
                {
                    NovelaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonajeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NovelaPersonaje", x => new { x.NovelaId, x.PersonajeId });
                    table.ForeignKey(
                        name: "FK_NovelaPersonaje_Novelas_NovelaId",
                        column: x => x.NovelaId,
                        principalTable: "Novelas",
                        principalColumn: "NovelaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NovelaPersonaje_Personajes_PersonajeId",
                        column: x => x.PersonajeId,
                        principalTable: "Personajes",
                        principalColumn: "PersonajeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonajeSprites",
                columns: table => new
                {
                    PersonajeSpriteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DireccionImagen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonajeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonajeSprites", x => x.PersonajeSpriteId);
                    table.ForeignKey(
                        name: "FK_PersonajeSprites_Personajes_PersonajeId",
                        column: x => x.PersonajeId,
                        principalTable: "Personajes",
                        principalColumn: "PersonajeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecursosDecision_BackgroundSpriteId",
                table: "RecursosDecision",
                column: "BackgroundSpriteId");

            migrationBuilder.CreateIndex(
                name: "IX_RecursosDecision_PersonajeSpriteId",
                table: "RecursosDecision",
                column: "PersonajeSpriteId");

            migrationBuilder.CreateIndex(
                name: "IX_RecursosConversacion_BackgroundSpriteId",
                table: "RecursosConversacion",
                column: "BackgroundSpriteId");

            migrationBuilder.CreateIndex(
                name: "IX_RecursosConversacion_PersonajeSpriteId",
                table: "RecursosConversacion",
                column: "PersonajeSpriteId");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundSprites_BackgroundId",
                table: "BackgroundSprites",
                column: "BackgroundId");

            migrationBuilder.CreateIndex(
                name: "IX_NovelaBackground_BackgroundId",
                table: "NovelaBackground",
                column: "BackgroundId");

            migrationBuilder.CreateIndex(
                name: "IX_NovelaPersonaje_PersonajeId",
                table: "NovelaPersonaje",
                column: "PersonajeId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonajeSprites_PersonajeId",
                table: "PersonajeSprites",
                column: "PersonajeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecursosConversacion_BackgroundSprites_BackgroundSpriteId",
                table: "RecursosConversacion",
                column: "BackgroundSpriteId",
                principalTable: "BackgroundSprites",
                principalColumn: "BackgroundSpriteId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecursosConversacion_PersonajeSprites_PersonajeSpriteId",
                table: "RecursosConversacion",
                column: "PersonajeSpriteId",
                principalTable: "PersonajeSprites",
                principalColumn: "PersonajeSpriteId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecursosDecision_BackgroundSprites_BackgroundSpriteId",
                table: "RecursosDecision",
                column: "BackgroundSpriteId",
                principalTable: "BackgroundSprites",
                principalColumn: "BackgroundSpriteId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecursosDecision_PersonajeSprites_PersonajeSpriteId",
                table: "RecursosDecision",
                column: "PersonajeSpriteId",
                principalTable: "PersonajeSprites",
                principalColumn: "PersonajeSpriteId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecursosConversacion_BackgroundSprites_BackgroundSpriteId",
                table: "RecursosConversacion");

            migrationBuilder.DropForeignKey(
                name: "FK_RecursosConversacion_PersonajeSprites_PersonajeSpriteId",
                table: "RecursosConversacion");

            migrationBuilder.DropForeignKey(
                name: "FK_RecursosDecision_BackgroundSprites_BackgroundSpriteId",
                table: "RecursosDecision");

            migrationBuilder.DropForeignKey(
                name: "FK_RecursosDecision_PersonajeSprites_PersonajeSpriteId",
                table: "RecursosDecision");

            migrationBuilder.DropTable(
                name: "BackgroundSprites");

            migrationBuilder.DropTable(
                name: "NovelaBackground");

            migrationBuilder.DropTable(
                name: "NovelaPersonaje");

            migrationBuilder.DropTable(
                name: "PersonajeSprites");

            migrationBuilder.DropTable(
                name: "Backgrounds");

            migrationBuilder.DropTable(
                name: "Personajes");

            migrationBuilder.DropIndex(
                name: "IX_RecursosDecision_BackgroundSpriteId",
                table: "RecursosDecision");

            migrationBuilder.DropIndex(
                name: "IX_RecursosDecision_PersonajeSpriteId",
                table: "RecursosDecision");

            migrationBuilder.DropIndex(
                name: "IX_RecursosConversacion_BackgroundSpriteId",
                table: "RecursosConversacion");

            migrationBuilder.DropIndex(
                name: "IX_RecursosConversacion_PersonajeSpriteId",
                table: "RecursosConversacion");

            migrationBuilder.DropColumn(
                name: "BackgroundSpriteId",
                table: "RecursosDecision");

            migrationBuilder.DropColumn(
                name: "PersonajeSpriteId",
                table: "RecursosDecision");

            migrationBuilder.DropColumn(
                name: "AutorMensaje",
                table: "RecursosConversacion");

            migrationBuilder.DropColumn(
                name: "BackgroundSpriteId",
                table: "RecursosConversacion");

            migrationBuilder.DropColumn(
                name: "PersonajeSpriteId",
                table: "RecursosConversacion");
        }
    }
}
