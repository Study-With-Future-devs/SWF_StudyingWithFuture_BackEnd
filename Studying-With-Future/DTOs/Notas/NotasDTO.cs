using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.DTOs.Notas
{
   // DTOs
    public class LancarNotaDTO
    {
        public int AlunoId { get; set; }
        public int AtividadeId { get; set; }
        
        [Range(0, 10, ErrorMessage = "A nota deve estar entre 0 e 10")]
        public decimal Valor { get; set; }
        
        [StringLength(500)]
        public string Observacao { get; set; }
    }

    public class UpdateNotaDTO
    {
        public int AlunoId { get; set; }
        public int AtividadeId { get; set; }
        
        [Range(0, 10, ErrorMessage = "A nota deve estar entre 0 e 10")]
        public decimal Valor { get; set; }
        
        [StringLength(500)]
        public string Observacao { get; set; }
    }

    public class NotaResponseDTO
    {
        public int AlunoId { get; set; }
        public string AlunoNome { get; set; }
        public int AtividadeId { get; set; }
        public string AtividadeTitulo { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataLancamento { get; set; }
        public string Observacao { get; set; }
        public int TurmaId { get; set; }
        public string TurmaCodigo { get; set; }
    }

    public class MediaResponseDTO
    {
        public int AlunoId { get; set; }
        public decimal Media { get; set; }
        public int TotalNotas { get; set; }
    }
}