using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.DTOs.Usuarios;
using Studying_With_Future.Models;

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
                    DisciplinaNome = c.Disciplina.Nome,
                    DataCriacao = c.DataCriacao,
                    Ativo = c.Ativo
                })
                .ToListAsync();

            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CoordenadorResponseDTO>> GetCoordenadoresById(int id)
        {
            var coordenadores = await _context.Coordenadores
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coordenadores == null)
            {
                return NotFound();
            }

            var response = new CoordenadorResponseDTO
            {
                Id = coordenadores.Id,
                Nome = coordenadores.Nome,
                Email = coordenadores.Email,
                TipoUsuario = nameof(Coordenador),
                Funcao = coordenadores.ObterFuncao(),
                AreaCoordenacao = coordenadores.AreaCoordenacao,
                DisciplinaId = coordenadores.DisciplinaId,
                DisciplinaNome = coordenadores.Disciplina.Nome,
                DataCriacao = coordenadores.DataCriacao,
                Ativo = coordenadores.Ativo
            };
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<CoordenadorResponseDTO>> CreateCoordenador(CoordenadorCreateDTO coordenadorCreateDTO)
        {
            if (await _context.Coordenadores.AnyAsync(c => c.Email == coordenadorCreateDTO.Email))
            {
                return BadRequest(new { message = "Email já cadastrado" });
            }

            var coordenador = new Coordenador
            {
                Nome = coordenadorCreateDTO.Nome,
                Email = coordenadorCreateDTO.Email,
                Senha = HashPassword(coordenadorCreateDTO.Senha),
                AreaCoordenacao = coordenadorCreateDTO.AreaCoordenacao,
                DisciplinaId = coordenadorCreateDTO.DisciplinaId
            };

            _context.Coordenadores.Add(coordenador);
            await _context.SaveChangesAsync();

            // 🔁 RECARREGA o coordenador com a disciplina incluída
            coordenador = await _context.Coordenadores
                .Include(c => c.Disciplina)
                .FirstOrDefaultAsync(c => c.Id == coordenador.Id);

            var response = new CoordenadorResponseDTO
            {
                Id = coordenador.Id,
                Nome = coordenador.Nome,
                Email = coordenador.Email,
                TipoUsuario = nameof(Coordenador),
                Funcao = coordenador.ObterFuncao(),
                AreaCoordenacao = coordenador.AreaCoordenacao,
                DisciplinaId = coordenador.DisciplinaId,
                DisciplinaNome = coordenador.Disciplina.Nome, // ✅ Agora não será null
                DataCriacao = coordenador.DataCriacao,
                Ativo = coordenador.Ativo
            };

            return CreatedAtAction(nameof(GetCoordenadoresById), new { id = coordenador.Id }, response);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCoordenador(int id, CoordenadorUpdateDTO coordenadorUpdateDTO)
        {
            if (id != coordenadorUpdateDTO.Id)
            {
                return BadRequest("ID do admin não corresponde");
            }

            var coordenador = await _context.Coordenadores.FindAsync(id);
            if (coordenador == null)
            {
                return NotFound();
            }

            if (await _context.Coordenadores.AnyAsync(c => c.Email == coordenadorUpdateDTO.Email && c.Id != id))
            {
                return BadRequest(new { message = "Email já cadastrado para outro usuário" });
            }
            coordenador.Nome = coordenadorUpdateDTO.Nome;
            coordenador.Email = coordenadorUpdateDTO.Email;
            coordenador.AreaCoordenacao = coordenadorUpdateDTO.AreaCoordenacao;

            // 6. Atualizar disciplina se fornecida
            if (coordenadorUpdateDTO.DisciplinaId.HasValue)
            {
                coordenador.DisciplinaId = coordenadorUpdateDTO.DisciplinaId.Value;
            }

            // 7. Atualizar senha apenas se for fornecida
            if (!string.IsNullOrEmpty(coordenadorUpdateDTO.Senha))
            {
                coordenador.Senha = HashPassword(coordenadorUpdateDTO.Senha);
            }

            // 8. Salvar changes
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CoordenadorExists(id))
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


        private async Task<bool> CoordenadorExists(int id)
        {
            return await _context.Coordenadores.AnyAsync(c => c.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoordenador(int id)
        {
            var coordenador = await _context.Coordenadores.FindAsync(id);
            if (coordenador == null)
            {
                return NotFound();
            }

            _context.Coordenadores.Remove(coordenador);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}