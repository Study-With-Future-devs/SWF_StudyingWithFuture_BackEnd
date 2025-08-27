using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Studying_With_Future.Controllers.Base;
using Studying_With_Future.Data;
using Studying_With_Future.Models;
using static Studying_With_Future.DTOs.DisciplinaDTO;

namespace Studying_With_Future.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisciplinasController : BaseController<Disciplina>
    {
        public DisciplinasController(AppDbContext context) : base(context) { }

         [HttpPost("com-dto")]
    public async Task<ActionResult<DisciplinaResponseDTO>> CreateComDto(DisciplinaCreateDTO dto)
    {
        var disciplina = new Disciplina
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao
        };

        _context.Disciplinas.Add(disciplina);
        await _context.SaveChangesAsync();

        return Ok(new DisciplinaResponseDTO
        {
            Id = disciplina.Id,
            Nome = disciplina.Nome,
            Descricao = disciplina.Descricao,
            QuantidadeTurmas = 0
        });
    }
    }
}