using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.Models;
using Studying_With_Future.DTOs.Usuarios;

namespace Studying_With_Future.Controllers.Usuarios
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDTO>>> GetUsuarios(
            [FromQuery] string tipo = null,
            [FromQuery] string search = null)
        {
            IQueryable<Usuario> query = _context.Usuarios.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tipo))
                query = query.Where(u => u.GetType().Name.Equals(tipo, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => EF.Functions.Like(u.Nome, $"%{search}%") || EF.Functions.Like(u.Email, $"%{search}%"));

            var usuarios = await query
                .Select(u => new UsuarioResponseDTO
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    TipoUsuario = u.GetType().Name,
                    Funcao = u.ObterFuncao()
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpGet("{id:int?}")]
        public async Task<ActionResult<UsuarioDetailResponseDTO>> GetUsuario(int? id)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.UsuarioTelas)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario is null)
                return NotFound(new { message = "Usuário não encontrado." });

            var response = new UsuarioDetailResponseDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                TipoUsuario = usuario.GetType().Name,
                Funcao = usuario.ObterFuncao(),
                TelasPermitidasIds = usuario.UsuarioTelas.Select(ut => ut.TelaId).ToList()
            };

            return Ok(response);
        }

        [HttpGet("{id:int?}/telas")]
        public async Task<ActionResult<IEnumerable<object>>> GetTelasUsuario(int? id)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.UsuarioTelas)
                .ThenInclude(ut => ut.Tela)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario is null)
                return NotFound(new { message = "Usuário não encontrado." });

            var telas = usuario.UsuarioTelas.Select(ut => new
            {
                ut.TelaId,
                ut.Tela.Nome,
                ut.Tela.Descricao
            });

            return Ok(telas);
        }

        [HttpDelete("{id:int?}")]
        public async Task<IActionResult> DeleteUsuario(int? id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario is null)
                return NotFound(new { message = "Usuário não encontrado." });

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private Task<bool> UsuarioExists(int? id)
        {
            return _context.Usuarios.AnyAsync(u => u.Id == id);
        }
    }
}
