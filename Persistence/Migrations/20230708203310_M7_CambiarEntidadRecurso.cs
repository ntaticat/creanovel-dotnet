using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class M7_CambiarEntidadRecurso : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "BackgroundSpriteId",
                table: "RecursosConversacion");

            migrationBuilder.DropColumn(
                name: "PersonajeSpriteId",
                table: "RecursosConversacion");

            migrationBuilder.AddColumn<string>(
                name: "AutorDecisionMensaje",
                table: "RecursosDecision",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutorDecisionMensaje",
                table: "RecursosDecision");

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

            migrationBuilder.AddForeignKey(
                name: "FK_RecursosConversacion_BackgroundSprites_BackgroundSpriteId",
                table: "RecursosConversacion",
                column: "BackgroundSpriteId",
                principalTable: "BackgroundSprites",
                principalColumn: "BackgroundSpriteId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecursosConversacion_PersonajeSprites_PersonajeSpriteId",
                table: "RecursosConversacion",
                column: "PersonajeSpriteId",
                principalTable: "PersonajeSprites",
                principalColumn: "PersonajeSpriteId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecursosDecision_BackgroundSprites_BackgroundSpriteId",
                table: "RecursosDecision",
                column: "BackgroundSpriteId",
                principalTable: "BackgroundSprites",
                principalColumn: "BackgroundSpriteId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecursosDecision_PersonajeSprites_PersonajeSpriteId",
                table: "RecursosDecision",
                column: "PersonajeSpriteId",
                principalTable: "PersonajeSprites",
                principalColumn: "PersonajeSpriteId");
        }
    }
}
