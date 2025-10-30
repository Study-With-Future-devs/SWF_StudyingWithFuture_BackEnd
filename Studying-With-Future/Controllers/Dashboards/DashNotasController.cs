using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.DTOs.Notas;
using Studying_With_Future.DTOs.Turmas;
using System.Linq;

namespace Studying_With_Future.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashNotasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashNotasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/dashboardnotas/geral
        // Retorna estatísticas gerais do sistema
        [HttpGet("geral")]
        public async Task<IActionResult> GetEstatisticasGerais()
        {
            var totalNotas = await _context.Notas.CountAsync();
            var mediaGeral = await _context.Notas.AverageAsync(n => (double?)n.Valor) ?? 0;
            var totalAlunos = await _context.Alunos.CountAsync();
            var totalTurmas = await _context.Turmas.CountAsync();

            var dados = new
            {
                TotalNotas = totalNotas,
                MediaGeral = Math.Round(mediaGeral, 2),
                TotalAlunos = totalAlunos,
                TotalTurmas = totalTurmas
            };

            return Ok(dados);
        }

        // GET: api/dashboardnotas/turma/{turmaId}
        // Retorna média geral da turma e ranking dos alunos
        [HttpGet("turma/{turmaId}")]
        public async Task<IActionResult> GetDashboardTurma(int turmaId)
        {
            var turma = await _context.Turmas
                .Include(t => t.Disciplina)
                .FirstOrDefaultAsync(t => t.Id == turmaId);

            if (turma == null)
                return NotFound("Turma não encontrada");

            var notas = await _context.Notas
                .Include(n => n.Aluno)
                .Include(n => n.Atividade)
                .Where(n => n.Atividade.TurmaId == turmaId)
                .ToListAsync();

            if (!notas.Any())
                return Ok(new { message = "Ainda não há notas lançadas para esta turma" });

            var mediaTurma = notas.Average(n => n.Valor);

            var ranking = notas
                .GroupBy(n => n.Aluno)
                .Select(g => new
                {
                    AlunoId = g.Key.Id,
                    AlunoNome = g.Key.Nome,
                    Media = Math.Round(g.Average(n => n.Valor), 2)
                })
                .OrderByDescending(x => x.Media)
                .Take(10)
                .ToList();

            var response = new
            {
                Turma = new
                {
                    turma.Id,
                    turma.Codigo,
                    turma.Descricao,
                    Disciplina = turma.Disciplina.Nome,
                    MediaTurma = Math.Round(mediaTurma, 2)
                },
                RankingTop10 = ranking
            };

            return Ok(response);
        }

        // GET: api/dashboardnotas/aluno/{alunoId}
        // Mostra evolução do aluno nas atividades
        [HttpGet("aluno/{alunoId}")]
        public async Task<IActionResult> GetDesempenhoAluno(int alunoId)
        {
            var aluno = await _context.Alunos.FindAsync(alunoId);
            if (aluno == null) return NotFound("Aluno não encontrado");

            var notas = await _context.Notas
                .Include(n => n.Atividade)
                .ThenInclude(a => a.Turma)
                .Where(n => n.AlunoId == alunoId)
                .OrderBy(n => n.DataLancamento)
                .Select(n => new
                {
                    n.AtividadeId,
                    AtividadeTitulo = n.Atividade.Titulo,
                    TurmaCodigo = n.Atividade.Turma.Codigo,
                    Nota = n.Valor,
                    Data = n.DataLancamento
                })
                .ToListAsync();

            if (!notas.Any())
                return Ok(new { message = "Aluno ainda não possui notas registradas" });

            var mediaGeral = Math.Round(notas.Average(n => n.Nota), 2);

            return Ok(new
            {
                Aluno = new { aluno.Id, aluno.Nome, aluno.Matricula },
                MediaGeral = mediaGeral,
                HistoricoNotas = notas
            });
        }

        // GET: api/dashboardnotas/distribuicao
        // Retorna distribuição de notas (quantas entre 0-4, 5-6, etc)
        [HttpGet("distribuicao")]
        public async Task<IActionResult> GetDistribuicaoNotas()
        {
            var notas = await _context.Notas.ToListAsync();

            if (!notas.Any())
                return Ok(new { message = "Nenhuma nota cadastrada ainda" });

            var distribuicao = new
            {
                Ruins = notas.Count(n => n.Valor < 5),
                Regulares = notas.Count(n => n.Valor >= 5 && n.Valor < 7),
                Boas = notas.Count(n => n.Valor >= 7 && n.Valor < 9),
                Excelentes = notas.Count(n => n.Valor >= 9)
            };

            return Ok(distribuicao);
        }

        // GET: api/dashboardnotas/atividade/{atividadeId}
        // Mostra médias e desempenho em uma atividade específica
        [HttpGet("atividade/{atividadeId}")]
        public async Task<IActionResult> GetDesempenhoAtividade(int atividadeId)
        {
            var atividade = await _context.Atividades
                .Include(a => a.Turma)
                .FirstOrDefaultAsync(a => a.Id == atividadeId);

            if (atividade == null)
                return NotFound("Atividade não encontrada");

            var notas = await _context.Notas
                .Include(n => n.Aluno)
                .Where(n => n.AtividadeId == atividadeId)
                .ToListAsync();

            if (!notas.Any())
                return Ok(new { message = "Nenhuma nota lançada para esta atividade" });

            var media = Math.Round(notas.Average(n => n.Valor), 2);
            var max = notas.Max(n => n.Valor);
            var min = notas.Min(n => n.Valor);

            var alunos = notas.Select(n => new
            {
                n.AlunoId,
                n.Aluno.Nome,
                n.Valor
            }).OrderByDescending(a => a.Valor);

            return Ok(new
            {
                Atividade = new
                {
                    atividade.Id,
                    atividade.Titulo,
                    atividade.Turma.Codigo,
                    atividade.DataEntrega
                },
                Media = media,
                MaiorNota = max,
                MenorNota = min,
                Notas = alunos
            });
        }
    }
}
