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
        public DbSet<RecursoConversacion> RecursosConversacion { get; set; }
        public DbSet<RecursoDecision> RecursosDecision { get; set; }
        public DbSet<RecursoDecisionOpcion> RecursoDecisionOpciones { get; set; }
        public DbSet<RecursoEntrada> RecursosEntrada { get; set; }
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
            modelBuilder.Entity<RecursoConversacion>().ToTable("RecursosConversacion");
            modelBuilder.Entity<RecursoDecision>().ToTable("RecursosDecision");
            modelBuilder.Entity<RecursoEntrada>().ToTable("RecursosEntrada");

            modelBuilder.Entity<Usuario>()
                .HasMany(e => e.NovelasCreadas)
                .WithOne(e => e.UsuarioCreador)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Lectura>()
                .HasOne(e => e.UsuarioPropietario)
                .WithMany(e => e.Lecturas)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LecturaRecursos>()
                .HasKey(lr => new { lr.LecturaId, lr.RecursoId });

            modelBuilder.Entity<Recurso>()
                .HasMany(e => e.Lecturas)
                .WithOne(e => e.Recurso)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<NovelaPersonaje>()
                .HasKey(np => new { np.NovelaId, np.PersonajeId });
            modelBuilder.Entity<NovelaBackground>()
                .HasKey(nb => new { nb.NovelaId, nb.BackgroundId });
        }
    }
}