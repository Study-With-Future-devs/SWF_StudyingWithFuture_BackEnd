using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.DTOs;
using Studying_With_Future.Models;

namespace Studying_With_Future.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioTelasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuarioTelasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/usuariotelas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioTelaResponseDTO>>> GetUsuarioTelas()
        {
            var usuarioTelas = await _context.UsuarioTelas
                .Include(ut => ut.Usuario)
                .Include(ut => ut.Tela)
                .Select(ut => new UsuarioTelaResponseDTO
                {
                    UsuarioId = ut.UsuarioId,
                    UsuarioNome = ut.Usuario.Nome,
                    UsuarioEmail = ut.Usuario.Email,
                    UsuarioTipo = ut.Usuario.GetType().Name,
                    TelaId = ut.TelaId,
                    TelaNome = ut.Tela.Nome,
                    TelaDescricao = ut.Tela.Descricao
                })
                .ToListAsync();

            return Ok(usuarioTelas);
        }

        // GET: api/usuariotelas/usuario/5
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<UsuarioTelaResponseDTO>>> GetTelasPorUsuario(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            var usuarioTelas = await _context.UsuarioTelas
                .Where(ut => ut.UsuarioId == usuarioId)
                .Include(ut => ut.Usuario)
                .Include(ut => ut.Tela)
                .Select(ut => new UsuarioTelaResponseDTO
                {
                    UsuarioId = ut.UsuarioId,
                    UsuarioNome = ut.Usuario.Nome,
                    UsuarioEmail = ut.Usuario.Email,
                    UsuarioTipo = ut.Usuario.GetType().Name,
                    TelaId = ut.TelaId,
                    TelaNome = ut.Tela.Nome,
                    TelaDescricao = ut.Tela.Descricao
                })
                .ToListAsync();

            return Ok(usuarioTelas);
        }

        // GET: api/usuariotelas/tela/5
        [HttpGet("tela/{telaId}")]
        public async Task<ActionResult<IEnumerable<UsuarioTelaResponseDTO>>> GetUsuariosPorTela(int telaId)
        {
            var tela = await _context.Telas.FindAsync(telaId);
            if (tela == null)
            {
                return NotFound(new { message = "Tela não encontrada" });
            }

            var usuarioTelas = await _context.UsuarioTelas
                .Where(ut => ut.TelaId == telaId)
                .Include(ut => ut.Usuario)
                .Include(ut => ut.Tela)
                .Select(ut => new UsuarioTelaResponseDTO
                {
                    UsuarioId = ut.UsuarioId,
                    UsuarioNome = ut.Usuario.Nome,
                    UsuarioEmail = ut.Usuario.Email,
                    UsuarioTipo = ut.Usuario.GetType().Name,
                    TelaId = ut.TelaId,
                    TelaNome = ut.Tela.Nome,
                    TelaDescricao = ut.Tela.Descricao
                })
                .ToListAsync();

            return Ok(usuarioTelas);
        }

        // POST: api/usuariotelas
        [HttpPost]
        public async Task<ActionResult<UsuarioTelaResponseDTO>> PostUsuarioTela(UsuarioTelaCreateDTO usuarioTelaCreateDTO)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioTelaCreateDTO.UsuarioId);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            var tela = await _context.Telas.FindAsync(usuarioTelaCreateDTO.TelaId);
            if (tela == null)
            {
                return NotFound(new { message = "Tela não encontrada" });
            }

            if (await _context.UsuarioTelas.AnyAsync(ut => ut.UsuarioId == usuarioTelaCreateDTO.UsuarioId && ut.TelaId == usuarioTelaCreateDTO.TelaId))
            {
                return BadRequest(new { message = "Esta associação já existe" });
            }

            var usuarioTela = new UsuarioTela
            {
                UsuarioId = usuarioTelaCreateDTO.UsuarioId,
                TelaId = usuarioTelaCreateDTO.TelaId
            };

            _context.UsuarioTelas.Add(usuarioTela);
            await _context.SaveChangesAsync();

            var response = new UsuarioTelaResponseDTO
            {
                UsuarioId = usuario.Id,
                UsuarioNome = usuario.Nome,
                UsuarioEmail = usuario.Email,
                UsuarioTipo = usuario.GetType().Name,
                TelaId = tela.Id,
                TelaNome = tela.Nome,
                TelaDescricao = tela.Descricao
            };

            return CreatedAtAction(nameof(GetUsuarioTelas), response);
        }

        // PUT: api/usuariotelas/usuario/5
        [HttpPut("usuario/{usuarioId}")]
        public async Task<IActionResult> PutTelasUsuario(int usuarioId, UsuarioTelasRequestDTO request)
        {
            if (usuarioId != request.UsuarioId)
            {
                return BadRequest("ID do usuário não corresponde");
            }

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            // Remove associações existentes
            var associacoesExistentes = await _context.UsuarioTelas
                .Where(ut => ut.UsuarioId == usuarioId)
                .ToListAsync();

            _context.UsuarioTelas.RemoveRange(associacoesExistentes);

            // Adiciona novas associações
            foreach (var telaId in request.TelaIds)
            {
                var tela = await _context.Telas.FindAsync(telaId);
                if (tela == null)
                {
                    return NotFound(new { message = $"Tela com ID {telaId} não encontrada" });
                }

                var usuarioTela = new UsuarioTela
                {
                    UsuarioId = usuarioId,
                    TelaId = telaId
                };

                _context.UsuarioTelas.Add(usuarioTela);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/usuariotelas
        [HttpDelete]
        public async Task<IActionResult> DeleteUsuarioTela(int usuarioId, int telaId)
        {
            var usuarioTela = await _context.UsuarioTelas
                .FirstOrDefaultAsync(ut => ut.UsuarioId == usuarioId && ut.TelaId == telaId);

            if (usuarioTela == null)
            {
                return NotFound();
            }

            _context.UsuarioTelas.Remove(usuarioTela);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}