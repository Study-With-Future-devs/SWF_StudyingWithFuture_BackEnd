using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Studying_With_Future.DTOs.Usuarios
{
        public class UsuarioCreateDTO
        {
            [Required]
            [StringLength(100)]
            public string Nome { get; set; }

            [Required]
            [EmailAddress]
            [StringLength(100)]
            public string Email { get; set; }

            [Required]
            [StringLength(255, MinimumLength = 6)]
            public string Senha { get; set; }

        }

        public class UsuarioResponseDTO
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public string Email { get; set; }
            public string TipoUsuario { get; set; }
            public string Funcao { get; set; }
        }
        
        public class UsuarioDetailResponseDTO : UsuarioResponseDTO
        {
            public DateTime? DataCriacao { get; set; }
            public bool Ativo { get; set; } = true;
            public List<int> TelasPermitidasIds { get; set; } = new List<int>();
        }
}