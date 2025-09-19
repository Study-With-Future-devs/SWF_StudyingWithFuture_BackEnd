using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.DTOs.Atividades
{
    public class AtividadeCreateDTO
    {
        [Required(ErrorMessage = "Título é obrigatório")]
        [StringLength(100, ErrorMessage = "Título não pode exceder 100 caracteres")]
        public string Titulo { get; set; }

        [StringLength(500, ErrorMessage = "Descrição não pode exceder 500 caracteres")]
        public string Descricao { get; set; }

        public DateTime? DataEntrega { get; set; }

        [Required(ErrorMessage = "Turma é obrigatória")]
        public int TurmaId { get; set; }
    }

    public class AtividadeUpdateDTO
    {
        [Required(ErrorMessage = "ID é obrigatório")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Título é obrigatório")]
        [StringLength(100, ErrorMessage = "Título não pode exceder 100 caracteres")]
        public string Titulo { get; set; }

        [StringLength(500, ErrorMessage = "Descrição não pode exceder 500 caracteres")]
        public string Descricao { get; set; }

        public DateTime? DataEntrega { get; set; }
    }

    public class AtividadeResponseDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataEntrega { get; set; }
        public int TurmaId { get; set; }
        public string TurmaCodigo { get; set; }
        public string DisciplinaNome { get; set; }
        public int QuantidadeNotas { get; set; }
    }

    public class AtividadeDetailResponseDTO : AtividadeResponseDTO
    {
        public List<NotaAtividadeDTO> Notas { get; set; } = new List<NotaAtividadeDTO>();
    }

    public class NotaAtividadeDTO
    {
        public int AlunoId { get; set; }
        public string AlunoNome { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataLancamento { get; set; }
        public string Observacao { get; set; }
    }
}