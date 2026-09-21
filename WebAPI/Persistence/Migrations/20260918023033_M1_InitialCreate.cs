using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Persistence.Migrations
{
    public partial class M1_InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Backgrounds",
                columns: table => new
                {
                    BackgroundId = table.Column<Guid>(type: "uuid", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backgrounds", x => x.BackgroundId);
                });

            migrationBuilder.CreateTable(
                name: "Personajes",
                columns: table => new
                {
                    PersonajeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personajes", x => x.PersonajeId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Novelas",
                columns: table => new
                {
                    NovelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Disponible = table.Column<bool>(type: "boolean", nullable: false),
                    UsuarioCreadorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Novelas", x => x.NovelaId);
                    table.ForeignKey(
                        name: "FK_Novelas_AspNetUsers_UsuarioCreadorId",
                        column: x => x.UsuarioCreadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BackgroundSprites",
                columns: table => new
                {
                    BackgroundSpriteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    DireccionImagen = table.Column<string>(type: "text", nullable: true),
                    BackgroundId = table.Column<Guid>(type: "uuid", nullable: false)
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
                name: "PersonajeSprites",
                columns: table => new
                {
                    PersonajeSpriteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    DireccionImagen = table.Column<string>(type: "text", nullable: true),
                    PersonajeId = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Lecturas",
                columns: table => new
                {
                    LecturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    NovelaRegistrosId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioPropietarioId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lecturas", x => x.LecturaId);
                    table.ForeignKey(
                        name: "FK_Lecturas_AspNetUsers_UsuarioPropietarioId",
                        column: x => x.UsuarioPropietarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lecturas_Novelas_NovelaRegistrosId",
                        column: x => x.NovelaRegistrosId,
                        principalTable: "Novelas",
                        principalColumn: "NovelaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NovelaBackground",
                columns: table => new
                {
                    NovelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    BackgroundId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    NovelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonajeId = table.Column<Guid>(type: "uuid", nullable: false)
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
                name: "NovelaVersiones",
                columns: table => new
                {
                    NovelaVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroVersion = table.Column<string>(type: "text", nullable: true),
                    Disponible = table.Column<bool>(type: "boolean", nullable: false),
                    NovelaId = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Escenas",
                columns: table => new
                {
                    EscenaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Identificador = table.Column<string>(type: "text", nullable: true),
                    NovelaVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimerEscena = table.Column<bool>(type: "boolean", nullable: false),
                    UltimaEscena = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escenas", x => x.EscenaId);
                    table.ForeignKey(
                        name: "FK_Escenas_NovelaVersiones_NovelaVersionId",
                        column: x => x.NovelaVersionId,
                        principalTable: "NovelaVersiones",
                        principalColumn: "NovelaVersionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recursos",
                columns: table => new
                {
                    RecursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    EscenaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimerRecurso = table.Column<bool>(type: "boolean", nullable: false),
                    UltimoRecurso = table.Column<bool>(type: "boolean", nullable: false),
                    TipoRecurso = table.Column<string>(type: "text", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "LecturaRecurso",
                columns: table => new
                {
                    LecturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecursoOrder = table.Column<int>(type: "integer", nullable: false)
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
                        principalColumn: "RecursoId");
                });

            migrationBuilder.CreateTable(
                name: "RecursosConversacion",
                columns: table => new
                {
                    RecursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mensaje = table.Column<string>(type: "text", nullable: true),
                    AutorMensaje = table.Column<string>(type: "text", nullable: true),
                    SiguienteRecursoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosConversacion", x => x.RecursoId);
                    table.ForeignKey(
                        name: "FK_RecursosConversacion_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecursosConversacion_Recursos_SiguienteRecursoId",
                        column: x => x.SiguienteRecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RecursosDecision",
                columns: table => new
                {
                    RecursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecisionMensaje = table.Column<string>(type: "text", nullable: true),
                    AutorDecisionMensaje = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosDecision", x => x.RecursoId);
                    table.ForeignKey(
                        name: "FK_RecursosDecision_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecursosEntrada",
                columns: table => new
                {
                    RecursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    mensajeAviso = table.Column<string>(type: "text", nullable: true),
                    nombreVariable = table.Column<string>(type: "text", nullable: true),
                    valorVariable = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosEntrada", x => x.RecursoId);
                    table.ForeignKey(
                        name: "FK_RecursosEntrada_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecursoDecisionOpciones",
                columns: table => new
                {
                    RecursoDecisionOpcionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpcionMensaje = table.Column<string>(type: "text", nullable: true),
                    SiguienteRecursoId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecursoDecisionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursoDecisionOpciones", x => x.RecursoDecisionOpcionId);
                    table.ForeignKey(
                        name: "FK_RecursoDecisionOpciones_Recursos_SiguienteRecursoId",
                        column: x => x.SiguienteRecursoId,
                        principalTable: "Recursos",
                        principalColumn: "RecursoId");
                    table.ForeignKey(
                        name: "FK_RecursoDecisionOpciones_RecursosDecision_RecursoDecisionId",
                        column: x => x.RecursoDecisionId,
                        principalTable: "RecursosDecision",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundSprites_BackgroundId",
                table: "BackgroundSprites",
                column: "BackgroundId");

            migrationBuilder.CreateIndex(
                name: "IX_Escenas_NovelaVersionId",
                table: "Escenas",
                column: "NovelaVersionId");

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
                name: "IX_NovelaBackground_BackgroundId",
                table: "NovelaBackground",
                column: "BackgroundId");

            migrationBuilder.CreateIndex(
                name: "IX_NovelaPersonaje_PersonajeId",
                table: "NovelaPersonaje",
                column: "PersonajeId");

            migrationBuilder.CreateIndex(
                name: "IX_Novelas_UsuarioCreadorId",
                table: "Novelas",
                column: "UsuarioCreadorId");

            migrationBuilder.CreateIndex(
                name: "IX_NovelaVersiones_NovelaId",
                table: "NovelaVersiones",
                column: "NovelaId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonajeSprites_PersonajeId",
                table: "PersonajeSprites",
                column: "PersonajeId");

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
                name: "IX_RecursosConversacion_SiguienteRecursoId",
                table: "RecursosConversacion",
                column: "SiguienteRecursoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BackgroundSprites");

            migrationBuilder.DropTable(
                name: "LecturaRecurso");

            migrationBuilder.DropTable(
                name: "NovelaBackground");

            migrationBuilder.DropTable(
                name: "NovelaPersonaje");

            migrationBuilder.DropTable(
                name: "PersonajeSprites");

            migrationBuilder.DropTable(
                name: "RecursoDecisionOpciones");

            migrationBuilder.DropTable(
                name: "RecursosConversacion");

            migrationBuilder.DropTable(
                name: "RecursosEntrada");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Lecturas");

            migrationBuilder.DropTable(
                name: "Backgrounds");

            migrationBuilder.DropTable(
                name: "Personajes");

            migrationBuilder.DropTable(
                name: "RecursosDecision");

            migrationBuilder.DropTable(
                name: "Recursos");

            migrationBuilder.DropTable(
                name: "Escenas");

            migrationBuilder.DropTable(
                name: "NovelaVersiones");

            migrationBuilder.DropTable(
                name: "Novelas");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
