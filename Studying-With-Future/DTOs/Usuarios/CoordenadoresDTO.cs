using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Studying_With_Future.Models;

namespace Studying_With_Future.DTOs.Usuarios
{
    public class CoordenadorCreateDTO
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

        [Required(ErrorMessage = "Área de coordenação é obrigatória")]
        [StringLength(100, ErrorMessage = "Área de coordenação não pode exceder 100 caracteres")]
        public string AreaCoordenacao { get; set; }

        [Required(ErrorMessage = "Disciplina é obrigatória")]
        public int DisciplinaId { get; set; }
    }

    public class CoordenadorResponseDTO
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string TipoUsuario { get; set; } = "Coordenador";

        [Required]
        public string Funcao { get; set; }

        [Required]
        public string AreaCoordenacao { get; set; }

        public int DisciplinaId { get; set; }
        public string DisciplinaNome { get; set; }

        public DateTime DataCriacao { get; set; }
        public bool Ativo { get; set; } = true;
    }
        

         public class CoordenadorUpdateDTO
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

        [Required(ErrorMessage = "Área de coordenação é obrigatória")]
        [StringLength(100, ErrorMessage = "Área de coordenação não pode exceder 100 caracteres")]
        public string AreaCoordenacao { get; set; }

        public int DisciplinaId { get; set; } // Nullable para ser opcional no update
    }
}