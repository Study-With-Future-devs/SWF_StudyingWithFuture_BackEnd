using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Models;

namespace Studying_With_Future.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets principais
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Coordenador> Coordenadores { get; set; }
        public DbSet<Professor> Professores { get; set; }
        public DbSet<Aluno> Alunos { get; set; }

        public DbSet<Tela> Telas { get; set; }
        public DbSet<UsuarioTela> UsuarioTelas { get; set; }

        public DbSet<Disciplina> Disciplinas { get; set; }
        public DbSet<Turma> Turmas { get; set; }
        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<Nota> Notas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da herança (TPH) para Usuario
            modelBuilder.Entity<Usuario>()
                .HasDiscriminator<string>("TipoUsuario")
                .HasValue<Admin>("Admin")
                .HasValue<Coordenador>("Coordenador")
                .HasValue<Professor>("Professor")
                .HasValue<Aluno>("Aluno");

            // Configuração de chaves compostas e relacionamentos
            modelBuilder.Entity<UsuarioTela>()
                .HasKey(ut => new { ut.UsuarioId, ut.TelaId });

            modelBuilder.Entity<UsuarioTela>()
                .HasOne(ut => ut.Usuario)
                .WithMany(u => u.UsuarioTelas)
                .HasForeignKey(ut => ut.UsuarioId);

            modelBuilder.Entity<UsuarioTela>()
                .HasOne(ut => ut.Tela)
                .WithMany(t => t.UsuarioTelas)
                .HasForeignKey(ut => ut.TelaId);

            // Relacionamento Professor-Turma (um professor para muitas turmas)
            modelBuilder.Entity<Turma>()
                .HasOne(t => t.Professor)
                .WithMany(p => p.Turmas)
                .HasForeignKey(t => t.ProfessorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento Aluno-Turma (muitos para muitos)
            modelBuilder.Entity<Aluno>()
                .HasMany(a => a.Turmas)
                .WithMany(t => t.Alunos)
                .UsingEntity<Dictionary<string, object>>(
                    "AlunoTurma",
                    j => j.HasOne<Turma>().WithMany().HasForeignKey("TurmaId"),
                    j => j.HasOne<Aluno>().WithMany().HasForeignKey("AlunoId"),
                    j => j.HasKey("AlunoId", "TurmaId"));

            // Relacionamento Atividade-Turma
            modelBuilder.Entity<Atividade>()
                .HasOne(a => a.Turma)
                .WithMany(t => t.Atividades)
                .HasForeignKey(a => a.TurmaId);

            // Relacionamento Nota-Aluno-Atividade
            modelBuilder.Entity<Nota>()
                .HasKey(n => new { n.AlunoId, n.AtividadeId });

            modelBuilder.Entity<Nota>()
                .HasOne(n => n.Aluno)
                .WithMany(a => a.Notas)
                .HasForeignKey(n => n.AlunoId);

            modelBuilder.Entity<Nota>()
                .HasOne(n => n.Atividade)
                .WithMany(a => a.Notas)
                .HasForeignKey(n => n.AtividadeId);
        }
    }
}