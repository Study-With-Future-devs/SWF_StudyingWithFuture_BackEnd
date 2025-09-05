using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.DTOs.Usuarios
{
    public class AlunoCreateDTO
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

        [Required(ErrorMessage = "Matrícula é obrigatória")]
        [StringLength(20, ErrorMessage = "Matrícula não pode exceder 20 caracteres")]
        public string Matricula { get; set; }

        [Required(ErrorMessage = "Período é obrigatório")]
        [StringLength(50, ErrorMessage = "Período não pode exceder 50 caracteres")]
        public string Periodo { get; set; }

        [Required(ErrorMessage = "Data de nascimento é obrigatória")]
        public DateTime DataNascimento { get; set; }
    }

    public class AlunoResponseDTO
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string TipoUsuario { get; set; } = "Aluno";

        [Required]
        public string Funcao { get; set; }

        public string Matricula { get; set; }
        public string Periodo { get; set; }
        public DateTime DataNascimento { get; set; }

        public DateTime DataCriacao { get; set; }
        public bool Ativo { get; set; } = true;
    }

    public class AlunoUpdateDTO
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

        [StringLength(20, ErrorMessage = "Matrícula não pode exceder 20 caracteres")]
        public string Matricula { get; set; }

        [StringLength(50, ErrorMessage = "Período não pode exceder 50 caracteres")]
        public string Periodo { get; set; }

        public DateTime? DataNascimento { get; set; }
    }
}