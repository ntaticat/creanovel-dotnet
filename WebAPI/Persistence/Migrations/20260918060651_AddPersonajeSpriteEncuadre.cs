using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class AddPersonajeSpriteEncuadre : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
