using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;

namespace Studying_With_Future.Middleware
{
    public class TelaAccessMiddleware
    {
        private readonly RequestDelegate _next;

        public TelaAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            // Verificar se o usuário está autenticado
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    try
                    {
                        // Buscar telas permitidas do usuário
                        var userTelas = await dbContext.UsuarioTelas
                            .Where(ut => ut.UsuarioId == userId)
                            .Select(ut => ut.TelaId)
                            .ToListAsync();

                        // Adicionar telas do usuário ao contexto para acesso nos controllers
                        context.Items["UserTelas"] = userTelas;

                        Console.WriteLine($"✅ Usuário {userId} tem acesso a {userTelas.Count} telas");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Erro ao buscar telas do usuário: {ex.Message}");
                    }
                }
            }

            await _next(context);
        }
    }
}