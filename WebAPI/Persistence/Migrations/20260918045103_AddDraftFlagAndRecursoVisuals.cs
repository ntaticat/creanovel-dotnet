using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class AddDraftFlagAndRecursoVisuals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecursoDecisionOpciones_Recursos_SiguienteRecursoId",
                table: "RecursoDecisionOpciones");

            migrationBuilder.AddColumn<Guid>(
                name: "BackgroundSpriteId",
                table: "Recursos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PersonajeSpriteId",
                table: "Recursos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsBorrador",
                table: "NovelaVersiones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PortadaImagenUrl",
                table: "Novelas",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_BackgroundSpriteId",
                table: "Recursos",
                column: "BackgroundSpriteId");

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_PersonajeSpriteId",
                table: "Recursos",
                column: "PersonajeSpriteId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecursoDecisionOpciones_Recursos_SiguienteRecursoId",
                table: "RecursoDecisionOpciones",
                column: "SiguienteRecursoId",
                principalTable: "Recursos",
                principalColumn: "RecursoId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Recursos_BackgroundSprites_BackgroundSpriteId",
                table: "Recursos",
                column: "BackgroundSpriteId",
                principalTable: "BackgroundSprites",
                principalColumn: "BackgroundSpriteId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Recursos_PersonajeSprites_PersonajeSpriteId",
                table: "Recursos",
                column: "PersonajeSpriteId",
                principalTable: "PersonajeSprites",
                principalColumn: "PersonajeSpriteId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecursoDecisionOpciones_Recursos_SiguienteRecursoId",
                table: "RecursoDecisionOpciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Recursos_BackgroundSprites_BackgroundSpriteId",
                table: "Recursos");

            migrationBuilder.DropForeignKey(
                name: "FK_Recursos_PersonajeSprites_PersonajeSpriteId",
                table: "Recursos");

            migrationBuilder.DropIndex(
                name: "IX_Recursos_BackgroundSpriteId",
                table: "Recursos");

            migrationBuilder.DropIndex(
                name: "IX_Recursos_PersonajeSpriteId",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "BackgroundSpriteId",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "PersonajeSpriteId",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "EsBorrador",
                table: "NovelaVersiones");

            migrationBuilder.DropColumn(
                name: "PortadaImagenUrl",
                table: "Novelas");

            migrationBuilder.AddForeignKey(
                name: "FK_RecursoDecisionOpciones_Recursos_SiguienteRecursoId",
                table: "RecursoDecisionOpciones",
                column: "SiguienteRecursoId",
                principalTable: "Recursos",
                principalColumn: "RecursoId");
        }
    }
}
