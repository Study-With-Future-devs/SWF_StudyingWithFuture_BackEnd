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
    public class AdminsController : ControllerBase
    {
        protected readonly AppDbContext _context;

        public AdminsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminResponseDTO>>> GetAdmins()
        {
            var admins = await _context.Admins
                .Select(a => new AdminResponseDTO
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Email = a.Email,
                    TipoUsuario = nameof(Admin),
                    Funcao = a.ObterFuncao(),
                    NivelAcesso = a.NivelAcesso,
                    DataCriacao = a.DataCriacao,
                    Ativo = a.Ativo
                })
                .ToListAsync();

            return Ok(admins);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminResponseDTO>> GetAdmin(int? id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return NotFound();

            var response = new AdminResponseDTO
            {
                Id = admin.Id,
                Nome = admin.Nome,
                Email = admin.Email,
                TipoUsuario = nameof(Admin),
                Funcao = admin.ObterFuncao(),
                NivelAcesso = admin.NivelAcesso,
                DataCriacao = admin.DataCriacao,
                Ativo = admin.Ativo
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<AdminResponseDTO>> PostAdmin(AdminCreateDTO adminCreateDTO)
        {
            if (await ValidationUtils.EmailExists(_context, adminCreateDTO.Email))
                return BadRequest(new { message = "Email já cadastrado" });

            var admin = new Admin
            {
                Nome = adminCreateDTO.Nome,
                Email = adminCreateDTO.Email,
                Senha = PasswordUtils.HashPassword(adminCreateDTO.Senha),
                NivelAcesso = "FullAccess" // padronizado
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            var response = new AdminResponseDTO
            {
                Id = admin.Id,
                Nome = admin.Nome,
                Email = admin.Email,
                TipoUsuario = nameof(Admin),
                Funcao = admin.ObterFuncao(),
                NivelAcesso = admin.NivelAcesso,
                DataCriacao = admin.DataCriacao,
                Ativo = true
            };

            return CreatedAtAction(nameof(GetAdmin), new { id = admin.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdmin(int? id, AdminUpdateDTO adminUpdateDTO)
        {
            if (id != adminUpdateDTO.Id) return BadRequest("ID do admin não corresponde");

            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return NotFound();

            if (await ValidationUtils.EmailExists(_context, adminUpdateDTO.Email, id))
                return BadRequest(new { message = "Email já cadastrado para outro usuário" });

            admin.Nome = adminUpdateDTO.Nome;
            admin.Email = adminUpdateDTO.Email;

            if (!string.IsNullOrEmpty(adminUpdateDTO.Senha))
                admin.Senha = PasswordUtils.HashPassword(adminUpdateDTO.Senha);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int? id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return NotFound();

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
