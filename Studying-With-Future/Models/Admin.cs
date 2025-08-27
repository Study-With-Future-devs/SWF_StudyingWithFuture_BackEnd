using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.Models
{
    public class Admin : Usuario
    {
        [StringLength(50)]
        public string NivelAcesso { get; set; }

        public override string ObterFuncao()
        {
            return "Administrador do Sistema";
        }    
    }
}