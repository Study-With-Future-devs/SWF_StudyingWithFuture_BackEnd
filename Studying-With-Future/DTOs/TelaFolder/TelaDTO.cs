using System.ComponentModel.DataAnnotations;

namespace Studying_With_Future.DTOs
{
    public class TelaCreateDTO
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(50, ErrorMessage = "Nome não pode exceder 50 caracteres")]
        public string Nome { get; set; }

        [StringLength(200, ErrorMessage = "Descrição não pode exceder 200 caracteres")]
        public string Descricao { get; set; }
    }

    public class TelaResponseDTO
    {
        public int Id { get; set; }
        
        [Required]
        public string Nome { get; set; }
        
        public string Descricao { get; set; }
        
        public int QuantidadeUsuarios { get; set; }
    }

    public class TelaUpdateDTO
    {
        [Required(ErrorMessage = "ID é obrigatório")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(50, ErrorMessage = "Nome não pode exceder 50 caracteres")]
        public string Nome { get; set; }

        [StringLength(200, ErrorMessage = "Descrição não pode exceder 200 caracteres")]
        public string Descricao { get; set; }
    }
}