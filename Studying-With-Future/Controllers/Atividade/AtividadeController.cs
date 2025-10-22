using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Studying_With_Future.Data;
using Studying_With_Future.Models;
using Studying_With_Future.DTOs.Atividades;

namespace Studying_With_Future.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class AtividadesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AtividadesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/atividades/turma/5
        [HttpGet("turma/{turmaId}")]
        public async Task<ActionResult<IEnumerable<AtividadeResponseDTO>>> GetAtividadesPorTurma(int turmaId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userRole = User.FindFirst(ClaimTypes.Role).Value;

            // Verificar permissões
            if (userRole == "Professor")
            {
                var turmaDoProfessor = await _context.Turmas
                    .AnyAsync(t => t.Id == turmaId && t.ProfessorId == userId);

                if (!turmaDoProfessor)
                {
                    return Forbid("❌ Você só pode visualizar atividades das suas turmas");
                }
            }

            var atividades = await _context.Atividades
                .Include(a => a.Turma)
                .ThenInclude(t => t.Disciplina)
                .Where(a => a.TurmaId == turmaId)
                .Select(a => new AtividadeResponseDTO
                {
                    Id = a.Id,
                    Titulo = a.Titulo,
                    Descricao = a.Descricao,
                    DataCriacao = a.DataCriacao,
                    DataEntrega = a.DataEntrega,
                    TurmaId = a.TurmaId,
                    TurmaCodigo = a.Turma.Codigo,
                    DisciplinaNome = a.Turma.Disciplina.Nome,
                    QuantidadeNotas = a.Notas.Count
                })
                .ToListAsync();

            return Ok(atividades);
        }

        // GET: api/atividades/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AtividadeDetailResponseDTO>> GetAtividade(int id)
        {
            var atividade = await _context.Atividades
                .Include(a => a.Turma)
                .ThenInclude(t => t.Disciplina)
                .Include(a => a.Notas)
                .ThenInclude(n => n.Aluno)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (atividade == null)
            {
                return NotFound();
            }

            var response = new AtividadeDetailResponseDTO
            {
                Id = atividade.Id,
                Titulo = atividade.Titulo,
                Descricao = atividade.Descricao,
                DataCriacao = atividade.DataCriacao,
                DataEntrega = atividade.DataEntrega,
                TurmaId = atividade.TurmaId,
                TurmaCodigo = atividade.Turma.Codigo,
                DisciplinaNome = atividade.Turma.Disciplina.Nome,
                Notas = atividade.Notas.Select(n => new NotaAtividadeDTO
                {
                    AlunoId = n.AlunoId,
                    AlunoNome = n.Aluno.Nome,
                    Valor = n.Valor,
                    DataLancamento = n.DataLancamento,
                    Observacao = n.Observacao
                }).ToList()
            };

            return Ok(response);
        }

        // POST: api/atividades
        [HttpPost]
        //[Authorize(Roles = "Professor,Admin,Coordenador")]
        public async Task<ActionResult<AtividadeResponseDTO>> CreateAtividade(AtividadeCreateDTO atividadeCreateDTO)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userRole = User.FindFirst(ClaimTypes.Role).Value;

            // Verificar se turma existe
            var turma = await _context.Turmas.FindAsync(atividadeCreateDTO.TurmaId);
            if (turma == null)
            {
                return NotFound("Turma não encontrada");
            }

            // Professor só pode criar atividades para suas turmas
            if (userRole == "Professor" && turma.ProfessorId != userId)
            {
                return Forbid("❌ Você só pode criar atividades para suas turmas");
            }

            var atividade = new Atividade
            {
                Titulo = atividadeCreateDTO.Titulo,
                Descricao = atividadeCreateDTO.Descricao,
                DataEntrega = atividadeCreateDTO.DataEntrega,
                TurmaId = atividadeCreateDTO.TurmaId
            };

            _context.Atividades.Add(atividade);
            await _context.SaveChangesAsync();

            // Recarregar com includes
            atividade = await _context.Atividades
                .Include(a => a.Turma)
                .ThenInclude(t => t.Disciplina)
                .FirstOrDefaultAsync(a => a.Id == atividade.Id);

            var response = new AtividadeResponseDTO
            {
                Id = atividade.Id,
                Titulo = atividade.Titulo,
                Descricao = atividade.Descricao,
                DataCriacao = atividade.DataCriacao,
                DataEntrega = atividade.DataEntrega,
                TurmaId = atividade.TurmaId,
                TurmaCodigo = atividade.Turma.Codigo,
                DisciplinaNome = atividade.Turma.Disciplina.Nome,
                QuantidadeNotas = 0
            };

            return CreatedAtAction(nameof(GetAtividade), new { id = atividade.Id }, response);
        }

        // PUT: api/atividades/5
        [HttpPut("{id}")]
        //[Authorize(Roles = "Professor,Admin,Coordenador")]
        public async Task<IActionResult> UpdateAtividade(int id, AtividadeUpdateDTO atividadeUpdateDTO)
        {
            if (id != atividadeUpdateDTO.Id)
            {
                return BadRequest("ID da atividade não corresponde");
            }

            var atividade = await _context.Atividades
                .Include(a => a.Turma)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (atividade == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userRole = User.FindFirst(ClaimTypes.Role).Value;

            // Professor só pode editar atividades de suas turmas
            if (userRole == "Professor" && atividade.Turma.ProfessorId != userId)
            {
                return Forbid("❌ Você só pode editar atividades das suas turmas");
            }

            atividade.Titulo = atividadeUpdateDTO.Titulo;
            atividade.Descricao = atividadeUpdateDTO.Descricao;
            atividade.DataEntrega = atividadeUpdateDTO.DataEntrega;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await AtividadeExists(id))
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

        // DELETE: api/atividades/5
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Professor,Admin,Coordenador")]
        public async Task<IActionResult> DeleteAtividade(int id)
        {
            var atividade = await _context.Atividades
                .Include(a => a.Turma)
                .Include(a => a.Notas)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (atividade == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userRole = User.FindFirst(ClaimTypes.Role).Value;

            // Professor só pode deletar atividades de suas turmas
            if (userRole == "Professor" && atividade.Turma.ProfessorId != userId)
            {
                return Forbid("❌ Você só pode deletar atividades das suas turmas");
            }

            if (atividade.Notas.Any())
            {
                return BadRequest(new { 
                    message = "Não é possível excluir a atividade pois existem notas associadas",
                    notasAssociadas = atividade.Notas.Count
                });
            }

            _context.Atividades.Remove(atividade);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> AtividadeExists(int id)
        {
            return await _context.Atividades.AnyAsync(e => e.Id == id);
        }
    }
}