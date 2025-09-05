using System.ComponentModel.DataAnnotations;

namespace Studying_With_Future.DTOs
{
    public class UsuarioTelaCreateDTO
    {
        [Required(ErrorMessage = "ID do usuário é obrigatório")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "ID da tela é obrigatório")]
        public int TelaId { get; set; }
    }

    public class UsuarioTelaResponseDTO
    {
        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; }
        public string UsuarioEmail { get; set; }
        public string UsuarioTipo { get; set; }
        
        public int TelaId { get; set; }
        public string TelaNome { get; set; }
        public string TelaDescricao { get; set; }
    }

    public class UsuarioTelasRequestDTO
    {
        [Required(ErrorMessage = "ID do usuário é obrigatório")]
        public int UsuarioId { get; set; }

        public List<int> TelaIds { get; set; } = new List<int>();
    }
}