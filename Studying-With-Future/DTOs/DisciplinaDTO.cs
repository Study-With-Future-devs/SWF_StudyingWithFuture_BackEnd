using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.DTOs
{
    public class DisciplinaDTO
    {
        
        public class DisciplinaCreateDTO
        {
            [Required]
            public string Nome { get; set; }
            
            public string Descricao { get; set; }
        }

        public class DisciplinaResponseDTO
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public string Descricao { get; set; }
            public int QuantidadeTurmas { get; set; }
        }
    }
}