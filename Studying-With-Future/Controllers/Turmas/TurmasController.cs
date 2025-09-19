using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Studying_With_Future.Data;
using Studying_With_Future.Models;
using Studying_With_Future.DTOs.Turmas;

namespace Studying_With_Future.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TurmasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TurmasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/turmas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TurmaResponseDTO>>> GetTurmas()
        {
            var turmas = await _context.Turmas
                .Include(t => t.Disciplina)
                .Include(t => t.Professor)
                .Include(t => t.AlunoTurmas)
                    .ThenInclude(at => at.Aluno)
                .Include(t => t.Atividades)
                .Select(t => new TurmaResponseDTO
                {
                    Id = t.Id,
                    Codigo = t.Codigo,
                    Descricao = t.Descricao,
                    DisciplinaId = t.DisciplinaId,
                    DisciplinaNome = t.Disciplina.Nome,
                    ProfessorId = t.ProfessorId,
                    ProfessorNome = t.Professor != null ? t.Professor.Nome : "Não atribuído",
                    QuantidadeAlunos = t.AlunoTurmas.Count,
                    QuantidadeAtividades = t.Atividades.Count
                })
                .ToListAsync();

            return Ok(turmas);
        }

        // GET: api/turmas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TurmaDetailResponseDTO>> GetTurma(int id)
        {
            var turma = await _context.Turmas
                .Include(t => t.Disciplina)
                .Include(t => t.Professor)
                .Include(t => t.AlunoTurmas)
                    .ThenInclude(at => at.Aluno)
                .Include(t => t.Atividades)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turma == null) return NotFound();

            var response = new TurmaDetailResponseDTO
            {
                Id = turma.Id,
                Codigo = turma.Codigo,
                Descricao = turma.Descricao,
                DisciplinaId = turma.DisciplinaId,
                DisciplinaNome = turma.Disciplina.Nome,
                ProfessorId = turma.ProfessorId,
                ProfessorNome = turma.Professor != null ? turma.Professor.Nome : "Não atribuído",
                Alunos = turma.AlunoTurmas.Select(at => new AlunoTurmaDTO
                {
                    Id = at.Aluno.Id,
                    Nome = at.Aluno.Nome,
                    Email = at.Aluno.Email,
                    Matricula = at.Aluno.Matricula
                }).ToList(),
                Atividades = turma.Atividades.Select(a => new AtividadeTurmaDTO
                {
                    Id = a.Id,
                    Titulo = a.Titulo,
                    DataCriacao = a.DataCriacao,
                    DataEntrega = a.DataEntrega
                }).ToList()
            };

            return Ok(response);
        }

        // POST: api/turmas
        [HttpPost]
        [Authorize(Roles = "Admin,Coordenador")]
        public async Task<ActionResult<TurmaResponseDTO>> CreateTurma(TurmaCreateDTO dto)
        {
            if (await _context.Turmas.AnyAsync(t => t.Codigo == dto.Codigo))
                return BadRequest(new { message = "Já existe uma turma com este código" });

            if (!await _context.Disciplinas.AnyAsync(d => d.Id == dto.DisciplinaId))
                return BadRequest(new { message = "Disciplina não encontrada" });

            var turma = new Turma
            {
                Codigo = dto.Codigo,
                Descricao = dto.Descricao,
                DisciplinaId = dto.DisciplinaId,
                ProfessorId = dto.ProfessorId
            };

            _context.Turmas.Add(turma);
            await _context.SaveChangesAsync();

            // Recarregar com includes
            turma = await _context.Turmas
                .Include(t => t.Disciplina)
                .Include(t => t.Professor)
                .FirstOrDefaultAsync(t => t.Id == turma.Id);

            var response = new TurmaResponseDTO
            {
                Id = turma.Id,
                Codigo = turma.Codigo,
                Descricao = turma.Descricao,
                DisciplinaId = turma.DisciplinaId,
                DisciplinaNome = turma.Disciplina.Nome,
                ProfessorId = turma.ProfessorId,
                ProfessorNome = turma.Professor != null ? turma.Professor.Nome : "Não atribuído",
                QuantidadeAlunos = 0,
                QuantidadeAtividades = 0
            };

            return CreatedAtAction(nameof(GetTurma), new { id = turma.Id }, response);
        }

        // PUT: api/turmas/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Coordenador")]
        public async Task<IActionResult> UpdateTurma(int id, TurmaUpdateDTO dto)
        {
            if (id != dto.Id) return BadRequest("ID da turma não corresponde");

            var turma = await _context.Turmas.FindAsync(id);
            if (turma == null) return NotFound();

            if (await _context.Turmas.AnyAsync(t => t.Codigo == dto.Codigo && t.Id != id))
                return BadRequest(new { message = "Já existe outra turma com este código" });

            turma.Codigo = dto.Codigo;
            turma.Descricao = dto.Descricao;
            turma.DisciplinaId = dto.DisciplinaId;
            turma.ProfessorId = dto.ProfessorId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TurmaExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/turmas/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTurma(int id)
        {
            var turma = await _context.Turmas
                .Include(t => t.AlunoTurmas)
                .Include(t => t.Atividades)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turma == null) return NotFound();

            if (turma.AlunoTurmas.Any())
                return BadRequest(new { 
                    message = "Não é possível excluir a turma pois existem alunos matriculados",
                    alunosMatriculados = turma.AlunoTurmas.Count
                });

            if (turma.Atividades.Any())
                return BadRequest(new { 
                    message = "Não é possível excluir a turma pois existem atividades associadas",
                    atividadesAssociadas = turma.Atividades.Count
                });

            _context.Turmas.Remove(turma);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/turmas/5/alunos/10
        [HttpPost("{turmaId}/alunos/{alunoId}")]
        [Authorize(Roles = "Admin,Coordenador,Professor")]
        public async Task<IActionResult> AdicionarAlunoTurma(int turmaId, int alunoId)
        {
            if (await _context.AlunoTurmas.AnyAsync(at => at.TurmaId == turmaId && at.AlunoId == alunoId))
                return BadRequest("Aluno já está matriculado nesta turma");

            var alunoTurma = new AlunoTurma
            {
                AlunoId = alunoId,
                TurmaId = turmaId
            };

            _context.AlunoTurmas.Add(alunoTurma);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Aluno matriculado com sucesso" });
        }

        // DELETE: api/turmas/5/alunos/10
        [HttpDelete("{turmaId}/alunos/{alunoId}")]
        [Authorize(Roles = "Admin,Coordenador,Professor")]
        public async Task<IActionResult> RemoverAlunoTurma(int turmaId, int alunoId)
        {
            var alunoTurma = await _context.AlunoTurmas
                .FirstOrDefaultAsync(at => at.TurmaId == turmaId && at.AlunoId == alunoId);

            if (alunoTurma == null)
                return NotFound("Aluno não encontrado nesta turma");

            _context.AlunoTurmas.Remove(alunoTurma);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Aluno removido da turma com sucesso" });
        }

        private async Task<bool> TurmaExists(int id)
        {
            return await _context.Turmas.AnyAsync(e => e.Id == id);
        }
    }
}
