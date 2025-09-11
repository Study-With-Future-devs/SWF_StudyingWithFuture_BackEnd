using System.ComponentModel.DataAnnotations;

namespace Studying_With_Future.DTOs.Auth
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email em formato inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Senha { get; set; }
    }

    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiraEm { get; set; }
        public UserInfoDTO Usuario { get; set; }
    }

    public class RefreshTokenDTO
    {
        [Required(ErrorMessage = "Refresh token é obrigatório")]
        public string RefreshToken { get; set; }
    }

    public class UserInfoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string TipoUsuario { get; set; }
        public string Funcao { get; set; }
        public List<int> TelasPermitidasIds { get; set; } = new List<int>();
    }
}