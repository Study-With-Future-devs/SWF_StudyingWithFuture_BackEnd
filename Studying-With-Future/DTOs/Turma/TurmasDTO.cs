using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.DTOs.Turmas
{
    public class TurmaCreateDTO
    {
        [Required(ErrorMessage = "Código é obrigatório")]
        [StringLength(50, ErrorMessage = "Código não pode exceder 50 caracteres")]
        public string Codigo { get; set; }

        [StringLength(200, ErrorMessage = "Descrição não pode exceder 200 caracteres")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "Disciplina é obrigatória")]
        public int DisciplinaId { get; set; }

        public int? ProfessorId { get; set; }
    }

    public class TurmaUpdateDTO
    {
        [Required(ErrorMessage = "ID é obrigatório")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Código é obrigatório")]
        [StringLength(50, ErrorMessage = "Código não pode exceder 50 caracteres")]
        public string Codigo { get; set; }

        [StringLength(200, ErrorMessage = "Descrição não pode exceder 200 caracteres")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "Disciplina é obrigatória")]
        public int DisciplinaId { get; set; }

        public int? ProfessorId { get; set; }
    }

    public class TurmaResponseDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Descricao { get; set; }
        public int DisciplinaId { get; set; }
        public string DisciplinaNome { get; set; }
        public int? ProfessorId { get; set; }
        public string ProfessorNome { get; set; }
        public int QuantidadeAlunos { get; set; }
        public int QuantidadeAtividades { get; set; }
    }

    public class TurmaDetailResponseDTO : TurmaResponseDTO
    {
        public List<AlunoTurmaDTO> Alunos { get; set; } = new List<AlunoTurmaDTO>();
        public List<AtividadeTurmaDTO> Atividades { get; set; } = new List<AtividadeTurmaDTO>();
    }

    public class AlunoTurmaDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Matricula { get; set; }
    }

    public class AtividadeTurmaDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataEntrega { get; set; }
    }
}