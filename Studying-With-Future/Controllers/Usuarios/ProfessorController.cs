using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.DTOs.Usuarios;
using Studying_With_Future.DTOs.Usuarios.Studying_With_Future.DTOs.Usuarios;
using Studying_With_Future.Models;

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
        public async Task<ActionResult<ProfessorResponseDTO>> GetProfessorById(int id)
        {
            var professor = await _context.Professores
                .FirstOrDefaultAsync(p => p.Id == id);

            if (professor == null)
            {
                return NotFound();
            }

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
        public async Task<ActionResult<ProfessorResponseDTO>> CreateProfessor(ProfessorCreateDTO professorCreateDTO)
        {
            if (await _context.Professores.AnyAsync(p => p.Email == professorCreateDTO.Email))
            {
                return BadRequest(new { message = "Email já cadastrado" });
            }

            var professor = new Professor
            {
                Nome = professorCreateDTO.Nome,
                Email = professorCreateDTO.Email,
                Senha = HashPassword(professorCreateDTO.Senha),
                Formacao = professorCreateDTO.Formacao,
                Especialidade = professorCreateDTO.Especialidade
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
        public async Task<IActionResult> UpdateProfessor(int id, ProfessorUpdateDTO professorUpdateDTO)
        {
            if (id != professorUpdateDTO.Id)
            {
                return BadRequest("ID do professor não corresponde");
            }

            var professor = await _context.Professores.FindAsync(id);
            if (professor == null)
            {
                return NotFound();
            }

            if (await _context.Professores.AnyAsync(p => p.Email == professorUpdateDTO.Email && p.Id != id))
            {
                return BadRequest(new { message = "Email já cadastrado para outro usuário" });
            }

            professor.Nome = professorUpdateDTO.Nome;
            professor.Email = professorUpdateDTO.Email;
            professor.Formacao = professorUpdateDTO.Formacao;
            professor.Especialidade = professorUpdateDTO.Especialidade;

            if (!string.IsNullOrEmpty(professorUpdateDTO.Senha))
            {
                professor.Senha = HashPassword(professorUpdateDTO.Senha);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProfessorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfessor(int id)
        {
            var professor = await _context.Professores.FindAsync(id);
            if (professor == null)
            {
                return NotFound();
            }

            _context.Professores.Remove(professor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private async Task<bool> ProfessorExists(int id)
        {
            return await _context.Professores.AnyAsync(p => p.Id == id);
        }
    }
}