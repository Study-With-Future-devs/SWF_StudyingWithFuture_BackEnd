using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.Models;

using Studying_With_Future.DTOs.Usuarios;

namespace Studying_With_Future.Controllers.Usuarios
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuariosController : ControllerBase
    {
        protected readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDTO>>> GetUsuarios(
            [FromQuery] string tipo = null, 
            [FromQuery] string search = null)
        {
            IQueryable<Usuario> query = _context.Usuarios;

            if (!string.IsNullOrEmpty(tipo))
            {
                query = query.Where(u => u.GetType().Name == tipo);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Nome.Contains(search) || u.Email.Contains(search));
            }

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

        // GET: api/usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDetailResponseDTO>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioTelas)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

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

        // GET: api/usuarios/5/telas
        [HttpGet("{id}/telas")]
        public async Task<ActionResult<object>> GetTelasUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioTelas)
                .ThenInclude(ut => ut.Tela)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            var telas = usuario.UsuarioTelas.Select(ut => new
            {
                ut.TelaId,
                ut.Tela.Nome,
                ut.Tela.Descricao
            });

            return Ok(telas);
        }

        // DELETE: api/usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        protected async Task<bool> UsuarioExists(int id)
        {
            return await _context.Usuarios.AnyAsync(e => e.Id == id);
        }
    }
}