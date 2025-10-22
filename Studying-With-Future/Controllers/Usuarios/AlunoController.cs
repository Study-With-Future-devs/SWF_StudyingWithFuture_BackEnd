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
    public class AlunoController : ControllerBase
    {
        protected readonly AppDbContext _context;

        public AlunoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlunoResponseDTO>>> GetAlunos()
        {
            var alunos = await _context.Alunos
                .Select(a => new AlunoResponseDTO
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Email = a.Email,
                    TipoUsuario = nameof(Aluno),
                    Funcao = a.ObterFuncao(),
                    Matricula = a.Matricula,
                    Periodo = a.Periodo,
                    DataNascimento = a.DataNascimento,
                    DataCriacao = a.DataCriacao,
                    Ativo = a.Ativo
                })
                .ToListAsync();

            return Ok(alunos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AlunoResponseDTO>> GetAlunoById(int? id)
        {
            var aluno = await _context.Alunos.FindAsync(id);
            if (aluno == null) return NotFound();

            var response = new AlunoResponseDTO
            {
                Id = aluno.Id,
                Nome = aluno.Nome,
                Email = aluno.Email,
                TipoUsuario = nameof(Aluno),
                Funcao = aluno.ObterFuncao(),
                Matricula = aluno.Matricula,
                Periodo = aluno.Periodo,
                DataNascimento = aluno.DataNascimento,
                DataCriacao = aluno.DataCriacao,
                Ativo = aluno.Ativo
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<AlunoResponseDTO>> CreateAluno(AlunoCreateDTO alunoCreateDTO)
        {
            if (await ValidationUtils.EmailExists(_context, alunoCreateDTO.Email))
                return BadRequest(new { message = "Email já cadastrado" });

            if (await ValidationUtils.MatriculaExists(_context, alunoCreateDTO.Matricula))
                return BadRequest(new { message = "Matrícula já cadastrada" });

            var aluno = new Aluno
            {
                Nome = alunoCreateDTO.Nome,
                Email = alunoCreateDTO.Email,
                Senha = PasswordUtils.HashPassword(alunoCreateDTO.Senha),
                Matricula = alunoCreateDTO.Matricula,
                Periodo = alunoCreateDTO.Periodo,
                DataNascimento = alunoCreateDTO.DataNascimento
            };

            _context.Alunos.Add(aluno);
            await _context.SaveChangesAsync();

            var response = new AlunoResponseDTO
            {
                Id = aluno.Id,
                Nome = aluno.Nome,
                Email = aluno.Email,
                TipoUsuario = nameof(Aluno),
                Funcao = aluno.ObterFuncao(),
                Matricula = aluno.Matricula,
                Periodo = aluno.Periodo,
                DataNascimento = aluno.DataNascimento,
                DataCriacao = aluno.DataCriacao,
                Ativo = aluno.Ativo
            };

            return CreatedAtAction(nameof(GetAlunoById), new { id = aluno.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAluno(int? id, AlunoUpdateDTO alunoUpdateDTO)
        {
            if (id != alunoUpdateDTO.Id) return BadRequest("ID do aluno não corresponde");

            var aluno = await _context.Alunos.FindAsync(id);
            if (aluno == null) return NotFound();

            if (await ValidationUtils.EmailExists(_context, alunoUpdateDTO.Email, id))
                return BadRequest(new { message = "Email já cadastrado para outro usuário" });

            if (!string.IsNullOrEmpty(alunoUpdateDTO.Matricula) && 
                await ValidationUtils.MatriculaExists(_context, alunoUpdateDTO.Matricula, id))
                return BadRequest(new { message = "Matrícula já cadastrada para outro aluno" });

            aluno.Nome = alunoUpdateDTO.Nome;
            aluno.Email = alunoUpdateDTO.Email;
            aluno.Matricula = alunoUpdateDTO.Matricula ?? aluno.Matricula;
            aluno.Periodo = alunoUpdateDTO.Periodo ?? aluno.Periodo;
            if (alunoUpdateDTO.DataNascimento.HasValue)
                aluno.DataNascimento = alunoUpdateDTO.DataNascimento.Value;

            if (!string.IsNullOrEmpty(alunoUpdateDTO.Senha))
                aluno.Senha = PasswordUtils.HashPassword(alunoUpdateDTO.Senha);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAluno(int? id)
        {
            var aluno = await _context.Alunos.FindAsync(id);
            if (aluno == null) return NotFound();

            _context.Alunos.Remove(aluno);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
