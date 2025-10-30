using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.DTOs.Dashboard;

namespace Studying_With_Future.Controllers;

[ApiController]
[Route("api/{controller}")]
[Authorize]
class DashboardController : ControllerBase
{
    public readonly AppDbContext _context;
    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("faltas-por-mes/{alunoID}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<FaltasPorMesDTO>>> GetFaltasPorMes(int alunoID)
    {
        var response = GetFaltasPorMesAsync(alunoID);
        return Ok(response);
    }

    [HttpGet("minhas-faltas-por-mes")]
    public async Task<ActionResult<IEnumerable<FaltasPorMesDTO>>> GetFaltasPorMesUsuario()
    {
        var userID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var response = GetFaltasPorMesAsync(userID);
        return Ok(response);
    }
    
    private async Task<List<FaltasPorMesDTO>> GetFaltasPorMesAsync(int userID)
    {
        var seisMesesAtras = DateTime.UtcNow.AddMonths(-5).Date;

        var faltas = await _context.frequencia
            .Where(f => !f.Presente && f.Data >= seisMesesAtras && f.AlunoId == userID)
            .GroupBy(f => new { f.Data.Year, f.Data.Month })
            .Select(g => new
            {
                Ano = g.Key.Year,
                Mes = g.Key.Month,
                TotalFaltas = g.Count()
            })
            .OrderBy(g => g.Ano)
            .ThenBy(g => g.Mes)
            .ToListAsync();

        var response = faltas.Select(f => new FaltasPorMesDTO
        {
            Mes = new DateTime(f.Ano, f.Mes, 1).ToString("MMM/yy", new CultureInfo("pt-BR")),
            TotalFaltas = f.TotalFaltas
        }).ToList();

        return response;
    }
}