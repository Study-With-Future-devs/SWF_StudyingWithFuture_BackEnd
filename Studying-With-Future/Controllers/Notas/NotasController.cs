// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.AspNetCore.Authorization;
// using System.Security.Claims;
// using Studying_With_Future.Data;
// using Studying_With_Future.Models;
// using Studying_With_Future.DTOs.Notas;

// namespace Studying_With_Future.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class NotasController : ControllerBase
//     {
//         private readonly AppDbContext _context;

//         public NotasController(AppDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/notas/professor/turma/5
//         [HttpGet("professor/turma/{turmaId}")]
//         [Authorize(Roles = "Professor,Admin,Coordenador")]
//         public async Task<ActionResult<IEnumerable<NotaResponseDTO>>> GetNotasPorTurma(int turmaId)
//         {
//             var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
//             var userRole = User.FindFirst(ClaimTypes.Role).Value;

//             // Para professores, verificar se a turma pertence a ele
//             if (userRole == "Professor")
//             {
//                 var turmaDoProfessor = await _context.Turmas
//                     .AnyAsync(t => t.Id == turmaId && t.ProfessorId == userId);

//                 if (!turmaDoProfessor)
//                 {
//                     return Forbid("❌ Você só pode visualizar notas das suas turmas");
//                 }
//             }

//             var notas = await _context.Notas
//                 .Include(n => n.Aluno)
//                 .Include(n => n.Atividade)
//                 .ThenInclude(a => a.Turma)
//                 .Where(n => n.Atividade.TurmaId == turmaId)
//                 .Select(n => new NotaResponseDTO
//                 {
//                     AlunoId = n.AlunoId,
//                     AlunoNome = n.Aluno.Nome,
//                     AtividadeId = n.AtividadeId,
//                     AtividadeTitulo = n.Atividade.Titulo,
//                     Valor = n.Valor,
//                     DataLancamento = n.DataLancamento,
//                     Observacao = n.Observacao,
//                     TurmaId = n.Atividade.TurmaId,
//                     TurmaCodigo = n.Atividade.Turma.Codigo
//                 })
//                 .ToListAsync();

//             return Ok(notas);
//         }

//         // GET: api/notas/aluno/5
//         [HttpGet("aluno/{alunoId}")]
//         public async Task<ActionResult<IEnumerable<NotaResponseDTO>>> GetNotasPorAluno(int alunoId)
//         {
//             var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
//             var userRole = User.FindFirst(ClaimTypes.Role).Value;

//             // Aluno só pode ver suas próprias notas
//             if (userRole == "Aluno" && userId != alunoId)
//             {
//                 return Forbid("❌ Você só pode visualizar suas próprias notas");
//             }

//             // Professor pode ver notas de alunos de suas turmas
//             if (userRole == "Professor")
//             {
//                 var alunoNaTurmaDoProfessor = await _context.Set<Aluno>()
//                     .AnyAsync(at => at.Id == alunoId &&
//                                    at.Turma.ProfessorId == userId);

//                 if (!alunoNaTurmaDoProfessor)
//                 {
//                     return Forbid("❌ Você só pode visualizar notas de alunos das suas turmas");
//                 }
//             }

//             var notas = await _context.Notas
//                 .Include(n => n.Aluno)
//                 .Include(n => n.Atividade)
//                 .ThenInclude(a => a.Turma)
//                 .Where(n => n.AlunoId == alunoId)
//                 .Select(n => new NotaResponseDTO
//                 {
//                     AlunoId = n.AlunoId,
//                     AlunoNome = n.Aluno.Nome,
//                     AtividadeId = n.AtividadeId,
//                     AtividadeTitulo = n.Atividade.Titulo,
//                     Valor = n.Valor,
//                     DataLancamento = n.DataLancamento,
//                     Observacao = n.Observacao,
//                     TurmaId = n.Atividade.TurmaId,
//                     TurmaCodigo = n.Atividade.Turma.Codigo
//                 })
//                 .ToListAsync();

//             return Ok(notas);
//         }

//         // GET: api/notas/aluno/5/atividade/10
//         [HttpGet("aluno/{alunoId}/atividade/{atividadeId}")]
//         public async Task<ActionResult<NotaResponseDTO>> GetNotaAlunoAtividade(int alunoId, int atividadeId)
//         {
//             var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
//             var userRole = User.FindFirst(ClaimTypes.Role).Value;

//             // Verificar permissões
//             if (!await TemPermissaoParaVerNota(userId, userRole, alunoId, atividadeId))
//             {
//                 return Forbid("❌ Acesso negado para visualizar esta nota");
//             }

//             var nota = await _context.Notas
//                 .Include(n => n.Aluno)
//                 .Include(n => n.Atividade)
//                 .ThenInclude(a => a.Turma)
//                 .FirstOrDefaultAsync(n => n.AlunoId == alunoId && n.AtividadeId == atividadeId);

//             if (nota == null)
//             {
//                 return NotFound("Nota não encontrada");
//             }

//             var response = new NotaResponseDTO
//             {
//                 AlunoId = nota.AlunoId,
//                 AlunoNome = nota.Aluno.Nome,
//                 AtividadeId = nota.AtividadeId,
//                 AtividadeTitulo = nota.Atividade.Titulo,
//                 Valor = nota.Valor,
//                 DataLancamento = nota.DataLancamento,
//                 Observacao = nota.Observacao,
//                 TurmaId = nota.Atividade.TurmaId,
//                 TurmaCodigo = nota.Atividade.Turma.Codigo
//             };

//             return Ok(response);
//         }

//         // POST: api/notas
//         [HttpPost]
//         [Authorize(Roles = "Professor,Admin")]
//         public async Task<ActionResult<NotaResponseDTO>> LancarNota(LancarNotaDTO lancarNotaDTO)
//         {
//             var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
//             var userRole = User.FindFirst(ClaimTypes.Role).Value;

//             // Validar se a atividade existe
//             var atividade = await _context.Atividades
//                 .Include(a => a.Turma)
//                 .FirstOrDefaultAsync(a => a.Id == lancarNotaDTO.AtividadeId);

//             if (atividade == null)
//             {
//                 return NotFound("Atividade não encontrada");
//             }

//             // Validar se o aluno existe
//             var aluno = await _context.Alunos.FindAsync(lancarNotaDTO.AlunoId);
//             if (aluno == null)
//             {
//                 return NotFound("Aluno não encontrado");
//             }

//             // Professor só pode lançar notas em suas turmas
//             if (userRole == "Professor" && atividade.Turma.ProfessorId != userId)
//             {
//                 return Forbid("❌ Você só pode lançar notas para suas turmas");
//             }

//             // Validar se aluno está matriculado na turma
//             var alunoMatriculado = await _context.Set<Aluno>()
//                 .AnyAsync(at => at.Id == lancarNotaDTO.AlunoId &&
//                                at.TurmaId == atividade.TurmaId);

//             if (!alunoMatriculado)
//             {
//                 return BadRequest("Aluno não está matriculado nesta turma");
//             }

//             // Validar valor da nota (0-10)
//             if (lancarNotaDTO.Valor < 0 || lancarNotaDTO.Valor > 10)
//             {
//                 return BadRequest("A nota deve estar entre 0 e 10");
//             }

//             var nota = new Nota
//             {
//                 AlunoId = lancarNotaDTO.AlunoId,
//                 AtividadeId = lancarNotaDTO.AtividadeId,
//                 Valor = lancarNotaDTO.Valor,
//                 Observacao = lancarNotaDTO.Observacao,
//                 DataLancamento = DateTime.UtcNow
//             };

//             // Verificar se já existe nota para atualizar
//             var notaExistente = await _context.Notas
//                 .FirstOrDefaultAsync(n => n.AlunoId == lancarNotaDTO.AlunoId &&
//                                          n.AtividadeId == lancarNotaDTO.AtividadeId);

//             if (notaExistente != null)
//             {
//                 notaExistente.Valor = lancarNotaDTO.Valor;
//                 notaExistente.Observacao = lancarNotaDTO.Observacao;
//                 notaExistente.DataLancamento = DateTime.UtcNow;
//                 _context.Notas.Update(notaExistente);
//             }
//             else
//             {
//                 _context.Notas.Add(nota);
//             }

//             await _context.SaveChangesAsync();

//             var response = new NotaResponseDTO
//             {
//                 AlunoId = nota.AlunoId,
//                 AlunoNome = aluno.Nome,
//                 AtividadeId = nota.AtividadeId,
//                 AtividadeTitulo = atividade.Titulo,
//                 Valor = nota.Valor,
//                 DataLancamento = nota.DataLancamento,
//                 Observacao = nota.Observacao,
//                 TurmaId = atividade.TurmaId,
//                 TurmaCodigo = atividade.Turma.Codigo
//             };

//             return CreatedAtAction(nameof(GetNotaAlunoAtividade),
//                 new { alunoId = nota.AlunoId, atividadeId = nota.AtividadeId }, response);
//         }

//         // PUT: api/notas
//         [HttpPut]
//         [Authorize(Roles = "Professor,Admin")]
//         public async Task<IActionResult> UpdateNota(UpdateNotaDTO updateNotaDTO)
//         {
//             var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
//             var userRole = User.FindFirst(ClaimTypes.Role).Value;

//             var nota = await _context.Notas
//                 .Include(n => n.Atividade)
//                 .ThenInclude(a => a.Turma)
//                 .FirstOrDefaultAsync(n => n.AlunoId == updateNotaDTO.AlunoId &&
//                                          n.AtividadeId == updateNotaDTO.AtividadeId);

//             if (nota == null)
//             {
//                 return NotFound("Nota não encontrada");
//             }

//             // Professor só pode editar notas de suas turmas
//             if (userRole == "Professor" && nota.Atividade.Turma.ProfessorId != userId)
//             {
//                 return Forbid("❌ Você só pode editar notas das suas turmas");
//             }

//             // Validar valor da nota
//             if (updateNotaDTO.Valor < 0 || updateNotaDTO.Valor > 10)
//             {
//                 return BadRequest("A nota deve estar entre 0 e 10");
//             }

//             nota.Valor = updateNotaDTO.Valor;
//             nota.Observacao = updateNotaDTO.Observacao;
//             nota.DataLancamento = DateTime.UtcNow;

//             _context.Notas.Update(nota);
//             await _context.SaveChangesAsync();

//             return NoContent();
//         }

//         // DELETE: api/notas/aluno/5/atividade/10
//         [HttpDelete("aluno/{alunoId}/atividade/{atividadeId}")]
//         [Authorize(Roles = "Professor,Admin")]
//         public async Task<IActionResult> DeleteNota(int alunoId, int atividadeId)
//         {
//             var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
//             var userRole = User.FindFirst(ClaimTypes.Role).Value;

//             var nota = await _context.Notas
//                 .Include(n => n.Atividade)
//                 .ThenInclude(a => a.Turma)
//                 .FirstOrDefaultAsync(n => n.AlunoId == alunoId && n.AtividadeId == atividadeId);

//             if (nota == null)
//             {
//                 return NotFound("Nota não encontrada");
//             }

//             // Professor só pode deletar notas de suas turmas
//             if (userRole == "Professor" && nota.Atividade.Turma.ProfessorId != userId)
//             {
//                 return Forbid("❌ Você só pode deletar notas das suas turmas");
//             }

//             _context.Notas.Remove(nota);
//             await _context.SaveChangesAsync();

//             return NoContent();
//         }

//         // GET: api/notas/aluno/5/media
//         [HttpGet("aluno/{alunoId}/media")]
//         public async Task<ActionResult<MediaResponseDTO>> GetMediaAluno(int alunoId)
//         {
//             var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
//             var userRole = User.FindFirst(ClaimTypes.Role).Value;

//             // Verificar permissões
//             if (userRole == "Aluno" && userId != alunoId)
//             {
//                 return Forbid("❌ Você só pode visualizar sua própria média");
//             }

//             if (userRole == "Professor")
//             {
//                 var alunoNaTurmaDoProfessor = await _context.Set<Aluno>()
//                     .AnyAsync(at => at.Id == alunoId &&
//                                    at.Turma.ProfessorId == userId);

//                 if (!alunoNaTurmaDoProfessor)
//                 {
//                     return Forbid("❌ Você só pode visualizar médias de alunos das suas turmas");
//                 }
//             }

//             var media = await _context.Notas
//                 .Where(n => n.AlunoId == alunoId)
//                 .AverageAsync(n => (double?)n.Valor) ?? 0;

//             var totalNotas = await _context.Notas
//                 .CountAsync(n => n.AlunoId == alunoId);

//             return Ok(new MediaResponseDTO
//             {
//                 AlunoId = alunoId,
//                 Media = Math.Round((decimal)media, 2),
//                 TotalNotas = totalNotas
//             });
//         }

//         private async Task<bool> TemPermissaoParaVerNota(int userId, string userRole, int alunoId, int atividadeId)
//         {
//             if (userRole == "Aluno")
//             {
//                 return userId == alunoId;
//             }

//             if (userRole == "Professor")
//             {
//                 return await _context.Notas
//                     .Include(n => n.Atividade)
//                     .ThenInclude(a => a.Turma)
//                     .AnyAsync(n => n.AlunoId == alunoId &&
//                                   n.AtividadeId == atividadeId &&
//                                   n.Atividade.Turma.ProfessorId == userId);
//             }

//             // Admin e Coordenador têm acesso total
//             return userRole == "Admin" || userRole == "Coordenador";
//         }
//     }
// }

    