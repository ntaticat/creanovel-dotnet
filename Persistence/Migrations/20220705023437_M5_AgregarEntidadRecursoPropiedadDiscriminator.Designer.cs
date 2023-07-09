﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Persistence;

namespace Persistence.Migrations
{
    [DbContext(typeof(CreanovelDbContext))]
    [Migration("20220705023437_M5_AgregarEntidadRecursoPropiedadDiscriminator")]
    partial class M5_AgregarEntidadRecursoPropiedadDiscriminator
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Domain.Models.Background", b =>
                {
                    b.Property<Guid>("BackgroundId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Descripcion")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("BackgroundId");

                    b.ToTable("Backgrounds");
                });

            modelBuilder.Entity("Domain.Models.BackgroundSprite", b =>
                {
                    b.Property<Guid>("BackgroundSpriteId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("BackgroundId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("DireccionImagen")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nombre")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("BackgroundSpriteId");

                    b.HasIndex("BackgroundId");

                    b.ToTable("BackgroundSprites");
                });

            modelBuilder.Entity("Domain.Models.Escena", b =>
                {
                    b.Property<Guid>("EscenaId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Identificador")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("NovelaId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("PrimerEscena")
                        .HasColumnType("bit");

                    b.Property<bool>("UltimaEscena")
                        .HasColumnType("bit");

                    b.HasKey("EscenaId");

                    b.HasIndex("NovelaId");

                    b.ToTable("Escenas");
                });

            modelBuilder.Entity("Domain.Models.Lectura", b =>
                {
                    b.Property<Guid>("LecturaId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("NovelaRegistrosId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UsuarioPropietarioId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("LecturaId");

                    b.HasIndex("NovelaRegistrosId");

                    b.HasIndex("UsuarioPropietarioId");

                    b.ToTable("Lecturas");
                });

            modelBuilder.Entity("Domain.Models.LecturaRecursos", b =>
                {
                    b.Property<Guid>("LecturaId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RecursoId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("RecursoOrder")
                        .HasColumnType("int");

                    b.HasKey("LecturaId", "RecursoId");

                    b.HasIndex("RecursoId");

                    b.ToTable("LecturaRecurso");
                });

            modelBuilder.Entity("Domain.Models.Novela", b =>
                {
                    b.Property<Guid>("NovelaId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Descripcion")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Disponible")
                        .HasColumnType("bit");

                    b.Property<string>("Titulo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UsuarioCreadorId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("NovelaId");

                    b.HasIndex("UsuarioCreadorId");

                    b.ToTable("Novelas");
                });

            modelBuilder.Entity("Domain.Models.NovelaBackground", b =>
                {
                    b.Property<Guid>("NovelaId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("BackgroundId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("NovelaId", "BackgroundId");

                    b.HasIndex("BackgroundId");

                    b.ToTable("NovelaBackground");
                });

            modelBuilder.Entity("Domain.Models.NovelaPersonaje", b =>
                {
                    b.Property<Guid>("NovelaId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PersonajeId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("NovelaId", "PersonajeId");

                    b.HasIndex("PersonajeId");

                    b.ToTable("NovelaPersonaje");
                });

            modelBuilder.Entity("Domain.Models.Personaje", b =>
                {
                    b.Property<Guid>("PersonajeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Nombre")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PersonajeId");

                    b.ToTable("Personajes");
                });

            modelBuilder.Entity("Domain.Models.PersonajeSprite", b =>
                {
                    b.Property<Guid>("PersonajeSpriteId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("DireccionImagen")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nombre")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("PersonajeId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("PersonajeSpriteId");

                    b.HasIndex("PersonajeId");

                    b.ToTable("PersonajeSprites");
                });

            modelBuilder.Entity("Domain.Models.Recurso", b =>
                {
                    b.Property<Guid>("RecursoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("EscenaId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("PrimerRecurso")
                        .HasColumnType("bit");

                    b.Property<string>("TipoRecurso")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("UltimoRecurso")
                        .HasColumnType("bit");

                    b.HasKey("RecursoId");

                    b.HasIndex("EscenaId");

                    b.ToTable("Recursos");
                });

            modelBuilder.Entity("Domain.Models.RecursoDecisionOpcion", b =>
                {
                    b.Property<Guid>("RecursoDecisionOpcionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OpcionMensaje")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("RecursoDecisionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("SiguienteRecursoId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("RecursoDecisionOpcionId");

                    b.HasIndex("RecursoDecisionId");

                    b.HasIndex("SiguienteRecursoId");

                    b.ToTable("RecursoDecisionOpciones");
                });

            modelBuilder.Entity("Domain.Models.Usuario", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Nombre")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole<System.Guid>", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Domain.Models.RecursoConversacion", b =>
                {
                    b.HasBaseType("Domain.Models.Recurso");

                    b.Property<string>("AutorMensaje")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("BackgroundSpriteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Mensaje")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("PersonajeSpriteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("SiguienteRecursoId")
                        .HasColumnType("uniqueidentifier");

                    b.HasIndex("BackgroundSpriteId");

                    b.HasIndex("PersonajeSpriteId");

                    b.HasIndex("SiguienteRecursoId");

                    b.ToTable("RecursosConversacion");
                });

            modelBuilder.Entity("Domain.Models.RecursoDecision", b =>
                {
                    b.HasBaseType("Domain.Models.Recurso");

                    b.Property<Guid?>("BackgroundSpriteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("DecisionMensaje")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("PersonajeSpriteId")
                        .HasColumnType("uniqueidentifier");

                    b.HasIndex("BackgroundSpriteId");

                    b.HasIndex("PersonajeSpriteId");

                    b.ToTable("RecursosDecision");
                });

            modelBuilder.Entity("Domain.Models.BackgroundSprite", b =>
                {
                    b.HasOne("Domain.Models.Background", "Background")
                        .WithMany("Sprites")
                        .HasForeignKey("BackgroundId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Background");
                });

            modelBuilder.Entity("Domain.Models.Escena", b =>
                {
                    b.HasOne("Domain.Models.Novela", "Novela")
                        .WithMany("Escenas")
                        .HasForeignKey("NovelaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Novela");
                });

            modelBuilder.Entity("Domain.Models.Lectura", b =>
                {
                    b.HasOne("Domain.Models.Novela", "NovelaRegistros")
                        .WithMany()
                        .HasForeignKey("NovelaRegistrosId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Models.Usuario", "UsuarioPropietario")
                        .WithMany("Lecturas")
                        .HasForeignKey("UsuarioPropietarioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NovelaRegistros");

                    b.Navigation("UsuarioPropietario");
                });

            modelBuilder.Entity("Domain.Models.LecturaRecursos", b =>
                {
                    b.HasOne("Domain.Models.Lectura", "Lectura")
                        .WithMany("Recursos")
                        .HasForeignKey("LecturaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Models.Recurso", "Recurso")
                        .WithMany("Lecturas")
                        .HasForeignKey("RecursoId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();

                    b.Navigation("Lectura");

                    b.Navigation("Recurso");
                });

            modelBuilder.Entity("Domain.Models.Novela", b =>
                {
                    b.HasOne("Domain.Models.Usuario", "UsuarioCreador")
                        .WithMany("NovelasCreadas")
                        .HasForeignKey("UsuarioCreadorId");

                    b.Navigation("UsuarioCreador");
                });

            modelBuilder.Entity("Domain.Models.NovelaBackground", b =>
                {
                    b.HasOne("Domain.Models.Background", "Background")
                        .WithMany("Novelas")
                        .HasForeignKey("BackgroundId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Models.Novela", "Novela")
                        .WithMany("Backgrounds")
                        .HasForeignKey("NovelaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Background");

                    b.Navigation("Novela");
                });

            modelBuilder.Entity("Domain.Models.NovelaPersonaje", b =>
                {
                    b.HasOne("Domain.Models.Novela", "Novela")
                        .WithMany("Personajes")
                        .HasForeignKey("NovelaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Models.Personaje", "Personaje")
                        .WithMany("Novelas")
                        .HasForeignKey("PersonajeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Novela");

                    b.Navigation("Personaje");
                });

            modelBuilder.Entity("Domain.Models.PersonajeSprite", b =>
                {
                    b.HasOne("Domain.Models.Personaje", "Personaje")
                        .WithMany("Sprites")
                        .HasForeignKey("PersonajeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Personaje");
                });

            modelBuilder.Entity("Domain.Models.Recurso", b =>
                {
                    b.HasOne("Domain.Models.Escena", "Escena")
                        .WithMany("Recursos")
                        .HasForeignKey("EscenaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Escena");
                });

            modelBuilder.Entity("Domain.Models.RecursoDecisionOpcion", b =>
                {
                    b.HasOne("Domain.Models.RecursoDecision", "RecursoDecision")
                        .WithMany("Opciones")
                        .HasForeignKey("RecursoDecisionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Models.Recurso", "SiguienteRecurso")
                        .WithMany()
                        .HasForeignKey("SiguienteRecursoId");

                    b.Navigation("RecursoDecision");

                    b.Navigation("SiguienteRecurso");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.HasOne("Domain.Models.Usuario", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.HasOne("Domain.Models.Usuario", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Models.Usuario", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.HasOne("Domain.Models.Usuario", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Models.RecursoConversacion", b =>
                {
                    b.HasOne("Domain.Models.BackgroundSprite", "BackgroundSprite")
                        .WithMany()
                        .HasForeignKey("BackgroundSpriteId");

                    b.HasOne("Domain.Models.PersonajeSprite", "PersonajeSprite")
                        .WithMany()
                        .HasForeignKey("PersonajeSpriteId");

                    b.HasOne("Domain.Models.Recurso", null)
                        .WithOne()
                        .HasForeignKey("Domain.Models.RecursoConversacion", "RecursoId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();

                    b.HasOne("Domain.Models.Recurso", "SiguienteRecurso")
                        .WithMany()
                        .HasForeignKey("SiguienteRecursoId");

                    b.Navigation("BackgroundSprite");

                    b.Navigation("PersonajeSprite");

                    b.Navigation("SiguienteRecurso");
                });

            modelBuilder.Entity("Domain.Models.RecursoDecision", b =>
                {
                    b.HasOne("Domain.Models.BackgroundSprite", "BackgroundSprite")
                        .WithMany()
                        .HasForeignKey("BackgroundSpriteId");

                    b.HasOne("Domain.Models.PersonajeSprite", "PersonajeSprite")
                        .WithMany()
                        .HasForeignKey("PersonajeSpriteId");

                    b.HasOne("Domain.Models.Recurso", null)
                        .WithOne()
                        .HasForeignKey("Domain.Models.RecursoDecision", "RecursoId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();

                    b.Navigation("BackgroundSprite");

                    b.Navigation("PersonajeSprite");
                });

            modelBuilder.Entity("Domain.Models.Background", b =>
                {
                    b.Navigation("Novelas");

                    b.Navigation("Sprites");
                });

            modelBuilder.Entity("Domain.Models.Escena", b =>
                {
                    b.Navigation("Recursos");
                });

            modelBuilder.Entity("Domain.Models.Lectura", b =>
                {
                    b.Navigation("Recursos");
                });

            modelBuilder.Entity("Domain.Models.Novela", b =>
                {
                    b.Navigation("Backgrounds");

                    b.Navigation("Escenas");

                    b.Navigation("Personajes");
                });

            modelBuilder.Entity("Domain.Models.Personaje", b =>
                {
                    b.Navigation("Novelas");

                    b.Navigation("Sprites");
                });

            modelBuilder.Entity("Domain.Models.Recurso", b =>
                {
                    b.Navigation("Lecturas");
                });

            modelBuilder.Entity("Domain.Models.Usuario", b =>
                {
                    b.Navigation("Lecturas");

                    b.Navigation("NovelasCreadas");
                });

            modelBuilder.Entity("Domain.Models.RecursoDecision", b =>
                {
                    b.Navigation("Opciones");
                });
#pragma warning restore 612, 618
        }
    }
}