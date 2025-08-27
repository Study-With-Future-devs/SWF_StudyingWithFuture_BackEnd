using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.Models
{
    public class Professor : Usuario
    {
        [StringLength(100)]
        public string? Formacao { get; set; }

        [StringLength(100)]
        public string? Especialidade { get; set; }
        public virtual List<Turma> Turmas { get; set; } = new List<Turma>();

        public override string ObterFuncao()
        {
            return $"Professor de {Especialidade}";
        }
    }
}