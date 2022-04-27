using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Migrations
{
    public partial class M4_AgregarEntidadEscenaPropiedadPrimeraUltima : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PrimerEscena",
                table: "Escenas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UltimaEscena",
                table: "Escenas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimerEscena",
                table: "Escenas");

            migrationBuilder.DropColumn(
                name: "UltimaEscena",
                table: "Escenas");
        }
    }
}
