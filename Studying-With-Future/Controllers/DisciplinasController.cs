using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.Models;
using Microsoft.AspNetCore.Authorization;

namespace Studying_With_Future.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisciplinasController : ControllerBase
    {
        protected readonly AppDbContext _context;
        
        public DisciplinasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/disciplinas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DisciplinaResponseDTO>>> GetAllDisciplina()
        {
            var disciplinas = await _context.Disciplinas
                .Select(d => new DisciplinaResponseDTO
                {
                    Id = d.Id,
                    Nome = d.Nome,
                    Descricao = d.Descricao,
                    QuantidadeTurmas = d.Turmas.Count // Agora funciona porque temos a relação
                })
                .ToListAsync();

            return Ok(disciplinas);
        }

        // GET: api/disciplinas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DisciplinaResponseDTO>> GetDisciplina(int id)
        {
            var disciplina = await _context.Disciplinas
                .Include(d => d.Turmas) // Inclui as turmas para contar
                .FirstOrDefaultAsync(d => d.Id == id);
                
            if (disciplina == null)
            {
                return NotFound();
            }

            var response = new DisciplinaResponseDTO
            {
                Id = disciplina.Id,
                Nome = disciplina.Nome,
                Descricao = disciplina.Descricao,
                QuantidadeTurmas = disciplina.Turmas.Count
            };
            
            return Ok(response);
        }

        // POST: api/disciplinas
        [HttpPost]
        //[Authorize(Roles = "Admin,Coordenador")] // Só admins e coordenadores podem criar
        public async Task<ActionResult<DisciplinaResponseDTO>> CreateDisciplina(DisciplinaCreateDTO disciplinaCreateDTO)
        {
            // Validar se já existe disciplina com mesmo nome
            if (await _context.Disciplinas.AnyAsync(d => d.Nome == disciplinaCreateDTO.Nome))
            {
                return BadRequest(new { message = "Já existe uma disciplina com este nome" });
            }

            var disciplina = new Disciplina
            {
                Nome = disciplinaCreateDTO.Nome,
                Descricao = disciplinaCreateDTO.Descricao
            };

            _context.Disciplinas.Add(disciplina);
            await _context.SaveChangesAsync();

            var response = new DisciplinaResponseDTO
            {
                Id = disciplina.Id,
                Nome = disciplina.Nome,
                Descricao = disciplina.Descricao,
                QuantidadeTurmas = 0
            };

            return CreatedAtAction(nameof(GetDisciplina), new { id = disciplina.Id }, response);
        }

        // PUT: api/disciplinas/5
        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin,Coordenador")] // Só admins e coordenadores podem editar
        public async Task<IActionResult> UpdateDisciplina(int id, DisciplinaUpdateDTO disciplinaUpdateDTO)
        {
            if (id != disciplinaUpdateDTO.Id)
            {
                return BadRequest("ID da disciplina não corresponde");
            }

            var disciplina = await _context.Disciplinas.FindAsync(id);
            if (disciplina == null)
            {
                return NotFound();
            }

            // Validar se outro disciplina tem o mesmo nome
            if (await _context.Disciplinas.AnyAsync(d => d.Nome == disciplinaUpdateDTO.Nome && d.Id != id))
            {
                return BadRequest(new { message = "Já existe outra disciplina com este nome" });
            }

            disciplina.Nome = disciplinaUpdateDTO.Nome;
            disciplina.Descricao = disciplinaUpdateDTO.Descricao;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await DisciplinaExists(id))
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

        // DELETE: api/disciplinas/5
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")] // Só admin pode deletar
        public async Task<IActionResult> DeleteDisciplina(int id)
        {
            var disciplina = await _context.Disciplinas
                .Include(d => d.Turmas) // Verificar se há turmas associadas
                .FirstOrDefaultAsync(d => d.Id == id);
                
            if (disciplina == null)
            {
                return NotFound();
            }

            // Verificar se há turmas associadas à disciplina
            if (disciplina.Turmas.Any())
            {
                return BadRequest(new { 
                    message = "Não é possível excluir a disciplina pois existem turmas associadas",
                    turmasAssociadas = disciplina.Turmas.Count
                });
            }

            _context.Disciplinas.Remove(disciplina);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> DisciplinaExists(int id)
        {
            return await _context.Disciplinas.AnyAsync(e => e.Id == id);
        }
    }
}