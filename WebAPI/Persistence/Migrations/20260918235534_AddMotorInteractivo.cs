using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMotorInteractivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LecturaRecurso",
                table: "LecturaRecurso");

            migrationBuilder.AddColumn<string>(
                name: "Condicion",
                table: "RecursoDecisionOpciones",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CondicionModo",
                table: "RecursoDecisionOpciones",
                type: "text",
                nullable: false,
                defaultValue: "ocultar");

            migrationBuilder.AddColumn<string>(
                name: "Efectos",
                table: "RecursoDecisionOpciones",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Orden",
                table: "RecursoDecisionOpciones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "RecursoDecisionOpciones",
                type: "text",
                nullable: false,
                defaultValue: "opcion");

            migrationBuilder.AddColumn<string>(
                name: "Definiciones",
                table: "NovelaVersiones",
                type: "jsonb",
                nullable: false,
                defaultValue: "{\"variables\":[]}");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Lecturas",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<Guid>(
                name: "NovelaVersionId",
                table: "Lecturas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecursoActualId",
                table: "Lecturas",
                type: "uuid",
                nullable: true);

            // Orden estable para las opciones existentes (antes se devolvían en orden físico, sin columna).
            migrationBuilder.Sql(@"
                UPDATE ""RecursoDecisionOpciones"" o
                SET ""Orden"" = sub.rn
                FROM (
                    SELECT ""RecursoDecisionOpcionId"",
                           ROW_NUMBER() OVER (PARTITION BY ""RecursoDecisionId"" ORDER BY ctid) - 1 AS rn
                    FROM ""RecursoDecisionOpciones""
                ) sub
                WHERE sub.""RecursoDecisionOpcionId"" = o.""RecursoDecisionOpcionId"";
            ");

            // La nueva clave es (LecturaId, RecursoOrder): se renumera por lectura para garantizar que sea única.
            migrationBuilder.Sql(@"
                UPDATE ""LecturaRecurso"" lr
                SET ""RecursoOrder"" = sub.rn
                FROM (
                    SELECT ""LecturaId"", ""RecursoId"",
                           ROW_NUMBER() OVER (PARTITION BY ""LecturaId"" ORDER BY ""RecursoOrder"", ""RecursoId"") AS rn
                    FROM ""LecturaRecurso""
                ) sub
                WHERE sub.""LecturaId"" = lr.""LecturaId"" AND sub.""RecursoId"" = lr.""RecursoId"";
            ");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LecturaRecurso",
                table: "LecturaRecurso",
                columns: new[] { "LecturaId", "RecursoOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LecturaRecurso",
                table: "LecturaRecurso");

            migrationBuilder.DropColumn(
                name: "Condicion",
                table: "RecursoDecisionOpciones");

            migrationBuilder.DropColumn(
                name: "CondicionModo",
                table: "RecursoDecisionOpciones");

            migrationBuilder.DropColumn(
                name: "Efectos",
                table: "RecursoDecisionOpciones");

            migrationBuilder.DropColumn(
                name: "Orden",
                table: "RecursoDecisionOpciones");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "RecursoDecisionOpciones");

            migrationBuilder.DropColumn(
                name: "Definiciones",
                table: "NovelaVersiones");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Lecturas");

            migrationBuilder.DropColumn(
                name: "NovelaVersionId",
                table: "Lecturas");

            migrationBuilder.DropColumn(
                name: "RecursoActualId",
                table: "Lecturas");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LecturaRecurso",
                table: "LecturaRecurso",
                columns: new[] { "LecturaId", "RecursoId" });
        }
    }
}
