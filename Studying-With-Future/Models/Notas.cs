using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.Models
{
    public class Nota
    {
        public int AlunoId { get; set; }
        public Aluno Aluno { get; set; }

        public int AtividadeId { get; set; }
        public Atividade Atividade { get; set; }

        [Range(0, 10)]
        public decimal Valor { get; set; }

        public DateTime DataLancamento { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string Observacao { get; set; }
    }
}