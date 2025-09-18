using System.ComponentModel.DataAnnotations;

    public class DisciplinaCreateDTO
    {
        [Required]
        public string Nome { get; set; }
        
        public string Descricao { get; set; }
    }

    public class DisciplinaUpdateDTO
    {
        [Required]
        public int Id { get; set; }

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