using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.Models;
using System.Threading.Tasks;

namespace Studying_With_Future.Utils
{
    public static class ValidationUtils
    {
        // Checa se email já existe, ignorando um usuário específico (útil para update)
        public static async Task<bool> EmailExists(AppDbContext context, string email, int? ignoreId = null)
        {
            return await context.Usuarios
                .AnyAsync(u => u.Email == email && (!ignoreId.HasValue || u.Id != ignoreId.Value));
        }

        // Checa se matrícula já existe (para Aluno), ignorando um aluno específico
        public static async Task<bool> MatriculaExists(AppDbContext context, string matricula, int? ignoreId = null)
        {
            return await context.Alunos
                .AnyAsync(a => a.Matricula == matricula && (!ignoreId.HasValue || a.Id != ignoreId.Value));
        }

        // Se futuramente quiser checar DisciplinaId repetida para coordenador
        public static async Task<bool> DisciplinaIdExists(AppDbContext context, int disciplinaId, int? ignoreId = null)
        {
            return await context.Coordenadores
                .AnyAsync(c => c.DisciplinaId == disciplinaId && (!ignoreId.HasValue || c.Id != ignoreId.Value));
        }
    }
}
