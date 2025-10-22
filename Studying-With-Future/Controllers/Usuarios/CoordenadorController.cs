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
    public class CoordenadorController : ControllerBase
    {
        protected readonly AppDbContext _context;

        public CoordenadorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CoordenadorResponseDTO>>> GetCoordenadores()
        {
            var coordenadores = await _context.Coordenadores
                .Include(c => c.Disciplina)
                .Select(c => new CoordenadorResponseDTO
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Email = c.Email,
                    TipoUsuario = nameof(Coordenador),
                    Funcao = c.ObterFuncao(),
                    AreaCoordenacao = c.AreaCoordenacao,
                    DisciplinaId = c.DisciplinaId,
                    DisciplinaNome = c.Disciplina != null ? c.Disciplina.Nome : null,
                    DataCriacao = c.DataCriacao,
                    Ativo = c.Ativo
                })
                .ToListAsync();

            return Ok(coordenadores);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CoordenadorResponseDTO>> GetCoordenadorById(int id)
        {
            var c = await _context.Coordenadores.Include(c => c.Disciplina).FirstOrDefaultAsync(c => c.Id == id);
            if (c == null) return NotFound();

            var response = new CoordenadorResponseDTO
            {
                Id = c.Id,
                Nome = c.Nome,
                Email = c.Email,
                TipoUsuario = nameof(Coordenador),
                Funcao = c.ObterFuncao(),
                AreaCoordenacao = c.AreaCoordenacao,
                DisciplinaId = c.DisciplinaId,
                DisciplinaNome = c.Disciplina != null ? c.Disciplina.Nome : null,
                DataCriacao = c.DataCriacao,
                Ativo = c.Ativo
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<CoordenadorResponseDTO>> CreateCoordenador(CoordenadorCreateDTO dto)
        {
            if (await ValidationUtils.EmailExists(_context, dto.Email))
                return BadRequest(new { message = "Email já cadastrado" });

            if (await _context.Disciplinas.FindAsync(dto.DisciplinaId) == null)
                return BadRequest(new { message = "DisciplinaId inválido" });

            var c = new Coordenador
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Senha = PasswordUtils.HashPassword(dto.Senha),
                AreaCoordenacao = dto.AreaCoordenacao,
                DisciplinaId = dto.DisciplinaId
            };

            _context.Coordenadores.Add(c);
            await _context.SaveChangesAsync();

            var response = new CoordenadorResponseDTO
            {
                Id = c.Id,
                Nome = c.Nome,
                Email = c.Email,
                TipoUsuario = nameof(Coordenador),
                Funcao = c.ObterFuncao(),
                AreaCoordenacao = c.AreaCoordenacao,
                DisciplinaId = c.DisciplinaId,
                DisciplinaNome = (await _context.Disciplinas.FindAsync(c.DisciplinaId))?.Nome,
                DataCriacao = c.DataCriacao,
                Ativo = c.Ativo
            };

            return CreatedAtAction(nameof(GetCoordenadorById), new { id = c.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCoordenador(int id, CoordenadorUpdateDTO dto)
        {
            if (id != dto.Id) return BadRequest("ID não corresponde");

            var c = await _context.Coordenadores.FindAsync(id);
            if (c == null) return NotFound();

            if (await ValidationUtils.EmailExists(_context, dto.Email, id))
                return BadRequest(new { message = "Email já cadastrado" });

            if (await _context.Disciplinas.FindAsync(dto.DisciplinaId) == null)
                return BadRequest(new { message = "DisciplinaId inválido" });


            c.Nome = dto.Nome;
            c.Email = dto.Email;
            c.AreaCoordenacao = dto.AreaCoordenacao;
            c.DisciplinaId = dto.DisciplinaId;

            if (!string.IsNullOrEmpty(dto.Senha))
                c.Senha = PasswordUtils.HashPassword(dto.Senha);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoordenador(int id)
        {
            var c = await _context.Coordenadores.FindAsync(id);
            if (c == null) return NotFound();

            _context.Coordenadores.Remove(c);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
