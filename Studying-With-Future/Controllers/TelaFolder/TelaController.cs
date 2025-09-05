using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.DTOs;
using Studying_With_Future.Models;

namespace Studying_With_Future.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TelasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/telas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TelaResponseDTO>>> GetTelas()
        {
            var telas = await _context.Telas
                .Include(t => t.UsuarioTelas)
                .Select(t => new TelaResponseDTO
                {
                    Id = t.Id,
                    Nome = t.Nome,
                    Descricao = t.Descricao,
                    QuantidadeUsuarios = t.UsuarioTelas.Count
                })
                .ToListAsync();

            return Ok(telas);
        }

        // GET: api/telas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TelaResponseDTO>> GetTela(int id)
        {
            var tela = await _context.Telas
                .Include(t => t.UsuarioTelas)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tela == null)
            {
                return NotFound();
            }

            var response = new TelaResponseDTO
            {
                Id = tela.Id,
                Nome = tela.Nome,
                Descricao = tela.Descricao,
                QuantidadeUsuarios = tela.UsuarioTelas.Count
            };

            return Ok(response);
        }

        // POST: api/telas
        [HttpPost]
        public async Task<ActionResult<TelaResponseDTO>> PostTela(TelaCreateDTO telaCreateDTO)
        {
            if (await _context.Telas.AnyAsync(t => t.Nome == telaCreateDTO.Nome))
            {
                return BadRequest(new { message = "Já existe uma tela com este nome" });
            }

            var tela = new Tela
            {
                Nome = telaCreateDTO.Nome,
                Descricao = telaCreateDTO.Descricao
            };

            _context.Telas.Add(tela);
            await _context.SaveChangesAsync();

            var response = new TelaResponseDTO
            {
                Id = tela.Id,
                Nome = tela.Nome,
                Descricao = tela.Descricao,
                QuantidadeUsuarios = 0
            };

            return CreatedAtAction(nameof(GetTela), new { id = tela.Id }, response);
        }

        // PUT: api/telas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTela(int id, TelaUpdateDTO telaUpdateDTO)
        {
            if (id != telaUpdateDTO.Id)
            {
                return BadRequest("ID da tela não corresponde");
            }

            var tela = await _context.Telas.FindAsync(id);
            if (tela == null)
            {
                return NotFound();
            }

            if (await _context.Telas.AnyAsync(t => t.Nome == telaUpdateDTO.Nome && t.Id != id))
            {
                return BadRequest(new { message = "Já existe outra tela com este nome" });
            }

            tela.Nome = telaUpdateDTO.Nome;
            tela.Descricao = telaUpdateDTO.Descricao;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TelaExists(id))
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

        // DELETE: api/telas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTela(int id)
        {
            var tela = await _context.Telas
                .Include(t => t.UsuarioTelas)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tela == null)
            {
                return NotFound();
            }

            if (tela.UsuarioTelas.Any())
            {
                return BadRequest(new { message = "Não é possível excluir a tela pois existem usuários associados" });
            }

            _context.Telas.Remove(tela);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> TelaExists(int id)
        {
            return await _context.Telas.AnyAsync(e => e.Id == id);
        }
    }
}