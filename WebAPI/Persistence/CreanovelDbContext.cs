using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace Persistence
{

    public class CreanovelDbContext: IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Lectura> Lecturas { get; set; }
        public DbSet<LecturaRecursos> LecturaRecurso { get; set; }
        public DbSet<Novela> Novelas { get; set; }
        public DbSet<NovelaVersion> NovelaVersiones { get; set; }
        public DbSet<Escena> Escenas { get; set; }
        public DbSet<Recurso> Recursos { get; set; }
        public DbSet<RecursoDecisionOpcion> RecursoDecisionOpciones { get; set; }
        public DbSet<Personaje> Personajes { get; set; }
        public DbSet<Background> Backgrounds { get; set; }
        public DbSet<PersonajeSprite> PersonajeSprites { get; set; }
        public DbSet<BackgroundSprite> BackgroundSprites { get; set; }
        public DbSet<NovelaPersonaje> NovelaPersonaje { get; set; }
        public DbSet<NovelaBackground> NovelaBackground { get; set; }


        public CreanovelDbContext(DbContextOptions<CreanovelDbContext> options): base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Recurso>().ToTable("Recursos");
            modelBuilder.Entity<Recurso>().Property(r => r.Contenido).HasColumnType("jsonb");

            modelBuilder.Entity<Usuario>()
                .HasMany(e => e.NovelasCreadas)
                .WithOne(e => e.UsuarioCreador)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Lectura>()
                .HasOne(e => e.UsuarioPropietario)
                .WithMany(e => e.Lecturas)
                .OnDelete(DeleteBehavior.Cascade);

            // La clave es (Lectura, Orden) y no (Lectura, Recurso): un jugador puede volver a pasar por
            // el mismo recurso (hubs, bucles), cada paso es una fila.
            modelBuilder.Entity<LecturaRecursos>()
                .HasKey(lr => new { lr.LecturaId, lr.RecursoOrder });

            modelBuilder.Entity<Lectura>().Property(l => l.Estado).HasColumnType("jsonb").IsRequired();
            modelBuilder.Entity<NovelaVersion>().Property(nv => nv.Definiciones).HasColumnType("jsonb").IsRequired();
            modelBuilder.Entity<RecursoDecisionOpcion>().Property(o => o.Condicion).HasColumnType("jsonb");
            modelBuilder.Entity<RecursoDecisionOpcion>().Property(o => o.Efectos).HasColumnType("jsonb");
            modelBuilder.Entity<RecursoDecisionOpcion>().Property(o => o.Region).HasColumnType("jsonb");
            modelBuilder.Entity<RecursoDecisionOpcion>().Property(o => o.Tipo).IsRequired();
            modelBuilder.Entity<RecursoDecisionOpcion>().Property(o => o.CondicionModo).IsRequired();

            modelBuilder.Entity<Recurso>()
                .HasMany(e => e.Lecturas)
                .WithOne(e => e.Recurso)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<Recurso>()
                .HasOne(r => r.SiguienteRecurso)
                .WithMany()
                .HasForeignKey(r => r.SiguienteRecursoId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Recurso>()
                .HasMany(r => r.Opciones)
                .WithOne(o => o.RecursoDecision)
                .HasForeignKey(o => o.RecursoDecisionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecursoDecisionOpcion>()
                .HasOne(o => o.SiguienteRecurso)
                .WithMany()
                .HasForeignKey(o => o.SiguienteRecursoId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Recurso>()
                .HasOne(r => r.BackgroundSprite)
                .WithMany()
                .HasForeignKey(r => r.BackgroundSpriteId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Recurso>().Property(r => r.Personajes).HasColumnType("jsonb").IsRequired();

            modelBuilder.Entity<Escena>().Property(e => e.Etiquetas).HasColumnType("jsonb").IsRequired();

            modelBuilder.Entity<NovelaPersonaje>()
                .HasKey(np => new { np.NovelaId, np.PersonajeId });
            modelBuilder.Entity<NovelaBackground>()
                .HasKey(nb => new { nb.NovelaId, nb.BackgroundId });
        }
    }
}