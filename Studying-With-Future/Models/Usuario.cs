using System.ComponentModel.DataAnnotations;

namespace Studying_With_Future.Models
{
    public abstract class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Senha { get; set; }

        public List<UsuarioTela> UsuarioTelas { get; set; } = new List<UsuarioTela>();
    }
}