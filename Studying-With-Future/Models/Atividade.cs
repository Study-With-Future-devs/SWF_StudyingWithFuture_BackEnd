using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.Models
{
    public class Atividade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Titulo { get; set; }

        [StringLength(500)]
        public string Descricao { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataEntrega { get; set; }

        public int TurmaId { get; set; }
        public Turma Turma { get; set; }

        public List<Nota> Notas { get; set; } = new List<Nota>();
    }
}