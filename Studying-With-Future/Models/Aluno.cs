using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.Models
{
     public class Aluno : Usuario
    {
        [StringLength(20)]
        public string Matricula { get; set; }

        [StringLength(50)]
        public string Periodo { get; set; }

        public DateTime DataNascimento { get; set; }

        // Relacionamento Many-to-Many com Turmas
        public virtual List<Turma> Turmas { get; set; } = new List<Turma>();

        // Relacionamento com Notas
        public virtual List<Nota> Notas { get; set; } = new List<Nota>();

        public override string ObterFuncao()
        {
            return $"Aluno do {Periodo} período";
        }

        // Método específico para Aluno
        public decimal CalcularMediaGeral()
        {
            return Notas.Any() ? Notas.Average(n => n.Valor) : 0;
        }
    }
}