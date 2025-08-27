using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.Models
{
    public class Coordenador : Usuario
    {
        [StringLength(100)]
        public string AreaCoordenacao { get; set; }

        // Relacionamento com Curso
        public int DisciplinaId { get; set; }
        public virtual Disciplina Disciplina { get; set; }

        public override string ObterFuncao()
        {
            return $"Coordenador de {AreaCoordenacao}";
        }
    }
}