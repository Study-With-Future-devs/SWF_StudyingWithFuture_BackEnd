using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.Models
{
    public class Aluno : Usuario
    {
        public List<Turma> Turmas { get; set; } = new List<Turma>();
        public List<Nota> Notas { get; set; } = new List<Nota>();
    }
}