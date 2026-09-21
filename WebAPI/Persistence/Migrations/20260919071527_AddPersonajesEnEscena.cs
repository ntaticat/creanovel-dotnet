using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonajesEnEscena : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Los personajes de un nodo pasan de un único PersonajeSpriteId (con recorte en el sprite) a una lista jsonb con su colocación.
            // El sprite ya no se recorta al registrarlo: quien tenía uno queda centrado y entero en el escenario (x 50, y 50, escala 1).
            migrationBuilder.AddColumn<string>(
                name: "Personajes",
                table: "Recursos",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.Sql(@"
                UPDATE ""Recursos""
                SET ""Personajes"" = jsonb_build_array(jsonb_build_object(
                    'personajeSpriteId', ""PersonajeSpriteId""::text, 'x', 50, 'y', 50, 'escala', 1, 'espejo', false))
                WHERE ""PersonajeSpriteId"" IS NOT NULL;");

            migrationBuilder.DropForeignKey(
                name: "FK_Recursos_PersonajeSprites_PersonajeSpriteId",
                table: "Recursos");

            migrationBuilder.DropIndex(
                name: "IX_Recursos_PersonajeSpriteId",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "PersonajeSpriteId",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "PosicionX",
                table: "PersonajeSprites");

            migrationBuilder.DropColumn(
                name: "PosicionY",
                table: "PersonajeSprites");

            migrationBuilder.DropColumn(
                name: "Zoom",
                table: "PersonajeSprites");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PersonajeSpriteId",
                table: "Recursos",
                type: "uuid",
                nullable: true);

            // Solo cabe un personaje por nodo: se conserva el primero.
            migrationBuilder.Sql(@"
                UPDATE ""Recursos""
                SET ""PersonajeSpriteId"" = (""Personajes""->0->>'personajeSpriteId')::uuid
                WHERE jsonb_array_length(""Personajes"") > 0;");

            migrationBuilder.DropColumn(
                name: "Personajes",
                table: "Recursos");

            migrationBuilder.AddColumn<double>(
                name: "PosicionX",
                table: "PersonajeSprites",
                type: "double precision",
                nullable: false,
                defaultValue: 50.0);

            migrationBuilder.AddColumn<double>(
                name: "PosicionY",
                table: "PersonajeSprites",
                type: "double precision",
                nullable: false,
                defaultValue: 50.0);

            migrationBuilder.AddColumn<double>(
                name: "Zoom",
                table: "PersonajeSprites",
                type: "double precision",
                nullable: false,
                defaultValue: 1.0);

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_PersonajeSpriteId",
                table: "Recursos",
                column: "PersonajeSpriteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recursos_PersonajeSprites_PersonajeSpriteId",
                table: "Recursos",
                column: "PersonajeSpriteId",
                principalTable: "PersonajeSprites",
                principalColumn: "PersonajeSpriteId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
