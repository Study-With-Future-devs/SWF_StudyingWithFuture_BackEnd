using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.Models
{
    public class Professor : Usuario
    {
        public List<Turma> Turmas { get; set; } = new List<Turma>();
    }
}