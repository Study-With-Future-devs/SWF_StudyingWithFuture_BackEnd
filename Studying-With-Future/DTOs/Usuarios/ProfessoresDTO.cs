
   using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.DTOs.Usuarios
{
    public class ProfessorCreateDTO
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome não pode ter mais de 100 caracteres")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email em formato inválido")]
        [StringLength(100, ErrorMessage = "Email não pode exceder 100 caracteres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 255 caracteres")]
        [DataType(DataType.Password)]
        public string Senha { get; set; }

        [StringLength(100, ErrorMessage = "Formação não pode exceder 100 caracteres")]
        public string Formacao { get; set; }

        [StringLength(100, ErrorMessage = "Especialidade não pode exceder 100 caracteres")]
        public string Especialidade { get; set; }
    }

    public class ProfessorResponseDTO
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string TipoUsuario { get; set; } = "Professor";

        [Required]
        public string Funcao { get; set; }

        public string Formacao { get; set; }
        public string Especialidade { get; set; }

        public DateTime DataCriacao { get; set; }
        public bool Ativo { get; set; } = true;
    }

    public class ProfessorUpdateDTO
    {
        [Required(ErrorMessage = "ID é obrigatório")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome não pode exceder 100 caracteres")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email em formato inválido")]
        [StringLength(100, ErrorMessage = "Email não pode exceder 100 caracteres")]
        public string Email { get; set; }

        [StringLength(255, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 255 caracteres")]
        [DataType(DataType.Password)]
        public string Senha { get; set; }

        [StringLength(100, ErrorMessage = "Formação não pode exceder 100 caracteres")]
        public string Formacao { get; set; }

        [StringLength(100, ErrorMessage = "Especialidade não pode exceder 100 caracteres")]
        public string Especialidade { get; set; }
    }
}
