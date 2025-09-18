using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.DTOs.Usuarios;
using Studying_With_Future.Models;

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

        // GET: api/admins
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

        // GET: api/admins/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminResponseDTO>> GetAdmin(int id)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Id == id);

            if (admin == null)
            {
                return NotFound();
            }

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

        // POST: api/admins
        [HttpPost]
        public async Task<ActionResult<AdminResponseDTO>> PostAdmin(AdminCreateDTO adminCreateDTO)
        {
            // Verificar se email já existe
            if (await _context.Usuarios.AnyAsync(u => u.Email == adminCreateDTO.Email))
            {
                return BadRequest(new { message = "Email já cadastrado" });
            }

            // Criar o admin com hash de senha
            var admin = new Admin
            {
                Nome = adminCreateDTO.Nome,
                Email = adminCreateDTO.Email,
                Senha = HashPassword(adminCreateDTO.Senha), // Hash da senha
                NivelAcesso = adminCreateDTO.NivelAcesso
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            // Retornar response DTO
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

        // PUT: api/admins/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdmin(int id, AdminUpdateDTO adminUpdateDTO)
        {
            if (id != adminUpdateDTO.Id)
            {
                return BadRequest("ID do admin não corresponde");
            }

            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            // Verificar se email já existe em outro usuário
            if (await _context.Usuarios.AnyAsync(u => u.Email == adminUpdateDTO.Email && u.Id != id))
            {
                return BadRequest(new { message = "Email já cadastrado para outro usuário" });
            }

            // Atualizar propriedades
            admin.Nome = adminUpdateDTO.Nome;
            admin.Email = adminUpdateDTO.Email;
            admin.NivelAcesso = adminUpdateDTO.NivelAcesso;

            // Atualizar senha apenas se for fornecida
            if (!string.IsNullOrEmpty(adminUpdateDTO.Senha))
            {
                admin.Senha = HashPassword(adminUpdateDTO.Senha);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UsuarioExists(id))
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

        // DELETE: api/admins/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Método para verificar se usuário existe
        protected async Task<bool> UsuarioExists(int id)
        {
            return await _context.Usuarios.AnyAsync(e => e.Id == id);
        }

        // Método para hash de senha (usando BCrypt)
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Método para verificar senha
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}