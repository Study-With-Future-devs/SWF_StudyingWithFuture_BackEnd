using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Studying_With_Future.Models;


namespace Studying_With_Future.Models
{
    public class Turma
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Codigo { get; set; }

        [StringLength(200)]
        public string Descricao { get; set; }

        public int DisciplinaId { get; set; }
        public Disciplina Disciplina { get; set; }

        public int? ProfessorId { get; set; }
        public Professor Professor { get; set; }

        // Relacionamento Many-to-Many com Alunos via AlunoTurma
        public List<AlunoTurma> AlunoTurmas { get; set; } = new List<AlunoTurma>();

        [NotMapped]
        public IEnumerable<Aluno> Alunos => AlunoTurmas.Select(at => at.Aluno);

        public List<Atividade> Atividades { get; set; } = new List<Atividade>();
    }
}