using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CreaNovelNETCore.Migrations
{
    public partial class PrimeraMigracion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(nullable: false),
                    Nombre = table.Column<string>(nullable: true),
                    Correo = table.Column<string>(nullable: true),
                    Nickname = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                });

            migrationBuilder.CreateTable(
                name: "Novelas",
                columns: table => new
                {
                    NovelaId = table.Column<Guid>(nullable: false),
                    Titulo = table.Column<string>(nullable: true),
                    Descripcion = table.Column<string>(nullable: true),
                    Disponible = table.Column<bool>(nullable: false),
                    UsuarioCreadorId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Novelas", x => x.NovelaId);
                    table.ForeignKey(
                        name: "FK_Novelas_Usuarios_UsuarioCreadorId",
                        column: x => x.UsuarioCreadorId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Escenas",
                columns: table => new
                {
                    EscenaId = table.Column<Guid>(nullable: false),
                    Identificador = table.Column<string>(nullable: true),
                    NovelaId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escenas", x => x.EscenaId);
                    table.ForeignKey(
                        name: "FK_Escenas_Novelas_NovelaId",
                        column: x => x.NovelaId,
                        principalTable: "Novelas",
                        principalColumn: "NovelaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lecturas",
                columns: table => new
                {
                    LecturaId = table.Column<Guid>(nullable: false),
                    NovelaRegistrosId = table.Column<Guid>(nullable: false),
                    UsuarioPropietarioId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lecturas", x => x.LecturaId);
                    table.ForeignKey(
                        name: "FK_Lecturas_Novelas_NovelaRegistrosId",
                        column: x => x.NovelaRegistrosId,
                        principalTable: "Novelas",
                        principalColumn: "NovelaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lecturas_Usuarios_UsuarioPropietarioId",
                        column: x => x.UsuarioPropietarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recursos",
                columns: table => new
                {
                    RecursoId = table.Column<Guid>(nullable: false),
                    EscenaId = table.Column<Guid>(nullable: false),
                    tipo_recurso = table.Column<string>(nullable: false),
                    Mensaje = table.Column<string>(nullable: true),
                    SiguienteRecursoId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recursos", x => x.RecursoId);
                    table.ForeignKey(
                        name: "FK_Recursos_Escenas_EscenaId",
                        column: x => x.EscenaId,
                        principalTable: "Escenas",
                        principalColumn: "EscenaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recursos_Recursos_SiguienteRecursoId",
                        column: x => x.SiguienteRecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LecturaRecurso",
                columns: table => new
                {
                    LecturaId = table.Column<Guid>(nullable: false),
                    RecursoId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturaRecurso", x => new { x.LecturaId, x.RecursoId });
                    table.ForeignKey(
                        name: "FK_LecturaRecurso_Lecturas_LecturaId",
                        column: x => x.LecturaId,
                        principalTable: "Lecturas",
                        principalColumn: "LecturaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LecturaRecurso_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecursoDecisionOpciones",
                columns: table => new
                {
                    RecursoDecisionOpcionId = table.Column<Guid>(nullable: false),
                    Mensaje = table.Column<string>(nullable: true),
                    SiguienteRecursoId = table.Column<Guid>(nullable: true),
                    RecursoDecisionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursoDecisionOpciones", x => x.RecursoDecisionOpcionId);
                    table.ForeignKey(
                        name: "FK_RecursoDecisionOpciones_Recursos_RecursoDecisionId",
                        column: x => x.RecursoDecisionId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecursoDecisionOpciones_Recursos_SiguienteRecursoId",
                        column: x => x.SiguienteRecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Escenas_NovelaId",
                table: "Escenas",
                column: "NovelaId");

            migrationBuilder.CreateIndex(
                name: "IX_LecturaRecurso_RecursoId",
                table: "LecturaRecurso",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lecturas_NovelaRegistrosId",
                table: "Lecturas",
                column: "NovelaRegistrosId");

            migrationBuilder.CreateIndex(
                name: "IX_Lecturas_UsuarioPropietarioId",
                table: "Lecturas",
                column: "UsuarioPropietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Novelas_UsuarioCreadorId",
                table: "Novelas",
                column: "UsuarioCreadorId");

            migrationBuilder.CreateIndex(
                name: "IX_RecursoDecisionOpciones_RecursoDecisionId",
                table: "RecursoDecisionOpciones",
                column: "RecursoDecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecursoDecisionOpciones_SiguienteRecursoId",
                table: "RecursoDecisionOpciones",
                column: "SiguienteRecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_EscenaId",
                table: "Recursos",
                column: "EscenaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_SiguienteRecursoId",
                table: "Recursos",
                column: "SiguienteRecursoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LecturaRecurso");

            migrationBuilder.DropTable(
                name: "RecursoDecisionOpciones");

            migrationBuilder.DropTable(
                name: "Lecturas");

            migrationBuilder.DropTable(
                name: "Recursos");

            migrationBuilder.DropTable(
                name: "Escenas");

            migrationBuilder.DropTable(
                name: "Novelas");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
