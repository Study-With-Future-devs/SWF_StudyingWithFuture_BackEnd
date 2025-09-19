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

        public DbSet<AlunoTurma> AlunoTurmas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar DeleteBehavior padrão como Restrict
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Configuração da herança (TPH) para Usuario
            modelBuilder.Entity<Usuario>()
                .HasDiscriminator<string>("TipoUsuario")
                .HasValue<Admin>("Admin")
                .HasValue<Coordenador>("Coordenador")
                .HasValue<Professor>("Professor")
                .HasValue<Aluno>("Aluno");

            // Configurações de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Nome).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Senha).HasMaxLength(255).IsRequired();
            });

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

            // Configurações de Aluno
            modelBuilder.Entity<Aluno>(entity =>
            {
                entity.HasIndex(a => a.Matricula).IsUnique();
                entity.Property(a => a.Matricula).HasMaxLength(20).IsRequired();
                entity.Property(a => a.Periodo).HasMaxLength(50);
            });

            // Relacionamento Professor-Turma
            modelBuilder.Entity<Turma>()
                .HasOne(t => t.Professor)
                .WithMany(p => p.Turmas)
                .HasForeignKey(t => t.ProfessorId);

            // Relacionamento Aluno-Turma (muitos para muitos)
            modelBuilder.Entity<Aluno>()
                .HasMany(a => a.Turmas)
                .WithMany(t => t.Alunos)
                .UsingEntity<Dictionary<string, object>>(
                    "AlunoTurma",
                    j => j.HasOne<Turma>().WithMany().HasForeignKey("TurmaId"),
                    j => j.HasOne<Aluno>().WithMany().HasForeignKey("AlunoId"),
                    j => j.HasKey("AlunoId", "TurmaId"));

            // Configurações de Turma
            modelBuilder.Entity<Turma>(entity =>
            {
                entity.HasIndex(t => t.Codigo).IsUnique();
                entity.Property(t => t.Codigo).HasMaxLength(50).IsRequired();
                entity.Property(t => t.Descricao).HasMaxLength(200);
            });

            // Relacionamento Atividade-Turma
            modelBuilder.Entity<Atividade>()
                .HasOne(a => a.Turma)
                .WithMany(t => t.Atividades)
                .HasForeignKey(a => a.TurmaId);

            // Configurações de Atividade
            modelBuilder.Entity<Atividade>(entity =>
            {
                entity.Property(a => a.Titulo).HasMaxLength(100).IsRequired();
                entity.Property(a => a.Descricao).HasMaxLength(500);
            });

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

            // Configurações de Nota
            modelBuilder.Entity<Nota>(entity =>
            {
                entity.Property(n => n.Valor).HasColumnType("decimal(4,2)").IsRequired();
                entity.Property(n => n.Observacao).HasMaxLength(500);
            });

            // Configurações de Disciplina
            modelBuilder.Entity<Disciplina>(entity =>
            {
                entity.Property(d => d.Nome).HasMaxLength(100).IsRequired();
                entity.Property(d => d.Descricao).HasMaxLength(500);
            });

            // Configurações de Tela
            modelBuilder.Entity<Tela>(entity =>
            {
                entity.Property(t => t.Nome).HasMaxLength(50).IsRequired();
                entity.Property(t => t.Descricao).HasMaxLength(200);
            });

            modelBuilder.Entity<AlunoTurma>()
       .HasKey(at => new { at.AlunoId, at.TurmaId });

            modelBuilder.Entity<AlunoTurma>()
                .HasOne(at => at.Aluno)
                .WithMany(a => a.AlunoTurmas)
                .HasForeignKey(at => at.AlunoId);

            modelBuilder.Entity<AlunoTurma>()
                .HasOne(at => at.Turma)
                .WithMany(t => t.AlunoTurmas)
                .HasForeignKey(at => at.TurmaId);
        }
    }
}