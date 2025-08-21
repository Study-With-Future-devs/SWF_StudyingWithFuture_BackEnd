using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.Models
{
    public class UsuarioTela
    {
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public int TelaId { get; set; }
        public Tela Tela { get; set; }
    }
}