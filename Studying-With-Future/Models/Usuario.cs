using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Studying_With_Future.Models
{
    public abstract class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Nome não pode exceder 100 caracteres.")]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100, ErrorMessage = "Email não pode exceder 100 caracteres.")]
        public string Email { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "Senha não pode exceder 255 caracteres.")]
        [JsonIgnore] // Não serializa a senha para JSON
        public string Senha { get; set; }

        public virtual List<UsuarioTela> UsuarioTelas { get; set; } = new List<UsuarioTela>();

         public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

         public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;


        public virtual string ObterTipoUsuario()
        {
            return this.GetType().Name;
        }

        public virtual bool TemAcessoTela(int telaId)
        {
            return UsuarioTelas.Any(ut => ut.TelaId == telaId);
        }

        public abstract string ObterFuncao();

    }
    

}