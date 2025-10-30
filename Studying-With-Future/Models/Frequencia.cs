using System.ComponentModel.DataAnnotations;

namespace Studying_With_Future.Models
{
    public class Frequencia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AlunoId { get; set; }
        public Aluno Aluno { get; set; }

        [Required]
        public int TurmaId { get; set; }
        public Turma Turma { get; set; }

        [Required]
        public DateTime Data { get; set; }

        public bool Presente { get; set; } = false;
    }
}