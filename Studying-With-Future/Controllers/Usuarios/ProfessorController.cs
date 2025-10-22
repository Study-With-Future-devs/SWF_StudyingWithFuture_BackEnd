using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.DTOs.Usuarios;
using Studying_With_Future.Models;
using Studying_With_Future.Utils;

namespace Studying_With_Future.Controllers.Usuarios
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfessorController : ControllerBase
    {
        protected readonly AppDbContext _context;

        public ProfessorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProfessorResponseDTO>>> GetProfessores()
        {
            var professores = await _context.Professores
                .Select(p => new ProfessorResponseDTO
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Email = p.Email,
                    TipoUsuario = nameof(Professor),
                    Funcao = p.ObterFuncao(),
                    Formacao = p.Formacao,
                    Especialidade = p.Especialidade,
                    DataCriacao = p.DataCriacao,
                    Ativo = p.Ativo
                })
                .ToListAsync();

            return Ok(professores);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProfessorResponseDTO>> GetProfessorById(int? id)
        {
            var professor = await _context.Professores.FindAsync(id);
            if (professor == null) return NotFound();

            var response = new ProfessorResponseDTO
            {
                Id = professor.Id,
                Nome = professor.Nome,
                Email = professor.Email,
                TipoUsuario = nameof(Professor),
                Funcao = professor.ObterFuncao(),
                Formacao = professor.Formacao,
                Especialidade = professor.Especialidade,
                DataCriacao = professor.DataCriacao,
                Ativo = professor.Ativo
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ProfessorResponseDTO>> CreateProfessor(ProfessorCreateDTO dto)
        {
            if (await ValidationUtils.EmailExists(_context, dto.Email))
                return BadRequest(new { message = "Email já cadastrado" });

            var professor = new Professor
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Senha = PasswordUtils.HashPassword(dto.Senha),
                Formacao = dto.Formacao,
                Especialidade = dto.Especialidade
            };

            _context.Professores.Add(professor);
            await _context.SaveChangesAsync();

            var response = new ProfessorResponseDTO
            {
                Id = professor.Id,
                Nome = professor.Nome,
                Email = professor.Email,
                TipoUsuario = nameof(Professor),
                Funcao = professor.ObterFuncao(),
                Formacao = professor.Formacao,
                Especialidade = professor.Especialidade,
                DataCriacao = professor.DataCriacao,
                Ativo = professor.Ativo
            };

            return CreatedAtAction(nameof(GetProfessorById), new { id = professor.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfessor(int? id, ProfessorUpdateDTO dto)
        {
            if (id != dto.Id) return BadRequest("ID do professor não corresponde");

            var professor = await _context.Professores.FindAsync(id);
            if (professor == null) return NotFound();

            if (await ValidationUtils.EmailExists(_context, dto.Email, id))
                return BadRequest(new { message = "Email já cadastrado para outro usuário" });

            professor.Nome = dto.Nome;
            professor.Email = dto.Email;
            professor.Formacao = dto.Formacao;
            professor.Especialidade = dto.Especialidade;

            if (!string.IsNullOrEmpty(dto.Senha))
                professor.Senha = PasswordUtils.HashPassword(dto.Senha);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfessor(int? id)
        {
            var professor = await _context.Professores.FindAsync(id);
            if (professor == null) return NotFound();

            _context.Professores.Remove(professor);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
