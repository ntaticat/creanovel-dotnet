using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models
{

    public class CreanovelDbContext: IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Lectura> Lecturas { get; set; }
        public DbSet<LecturaRecursos> LecturaRecurso { get; set; }
        public DbSet<Novela> Novelas { get; set; }
        public DbSet<Escena> Escenas { get; set; }
        public DbSet<Recurso> Recursos { get; set; }
        public DbSet<RecursoConversacion> RecursosConversacion { get; set; }
        public DbSet<RecursoDecision> RecursosDecision { get; set; }
        public DbSet<RecursoDecisionOpcion> RecursoDecisionOpciones { get; set; }

        public CreanovelDbContext(DbContextOptions<CreanovelDbContext> options): base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Recurso>().ToTable("Recursos");
            modelBuilder.Entity<RecursoConversacion>().ToTable("RecursosConversacion");
            modelBuilder.Entity<RecursoDecision>().ToTable("RecursosDecision");

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
        }
    }
}