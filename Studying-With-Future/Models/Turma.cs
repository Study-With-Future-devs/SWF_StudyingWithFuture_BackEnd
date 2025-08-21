using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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

        public List<Aluno> Alunos { get; set; } = new List<Aluno>();
        public List<Atividade> Atividades { get; set; } = new List<Atividade>();
    }
}