using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.Models;

namespace Studying_With_Future.Services
{
    public class ExcelImportService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ExcelImportService> _logger;

        public ExcelImportService(AppDbContext context, ILogger<ExcelImportService> logger)
        {
            _context = context;
            _logger = logger;
            // Não precisa de configuração de licença com ClosedXML
        }

        public async Task<ImportResult> ImportUsersFromExcel(Stream fileStream)
        {
            var result = new ImportResult();
            var usersToAdd = new List<Usuario>();

            try
            {
                using (var workbook = new XLWorkbook(fileStream))
                {
                    var worksheet = workbook.Worksheet(1); // Primeira planilha
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Pular cabeçalho

                    foreach (var row in rows)
                    {
                        try
                        {
                            var user = await ParseUserFromRow(row);
                            if (user != null)
                            {
                                usersToAdd.Add(user);
                            }
                        }
                        catch (Exception ex)
                        {
                            result.AddError($"Linha {row.RowNumber()}: {ex.Message}");
                        }
                    }
                }

                // Salvar todos os usuários válidos
                if (usersToAdd.Any())
                {
                    await _context.Usuarios.AddRangeAsync(usersToAdd);
                    await _context.SaveChangesAsync();
                    result.ImportedCount = usersToAdd.Count;
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante importação de usuários via Excel");
                result.AddError($"Erro na importação: {ex.Message}");
            }

            return result;
        }

        private async Task<Usuario> ParseUserFromRow(IXLRangeRow row)

        {
            var nome = row.Cell(1).GetValue<string>()?.Trim();
            var email = row.Cell(2).GetValue<string>()?.Trim().ToLower();
            var telefone = row.Cell(4).GetValue<string>()?.Trim();
            var tipoStr = row.Cell(5).GetValue<string>()?.Trim();
            var matricula = row.Cell(6).GetValue<string>()?.Trim();
            var curso = row.Cell(7).GetValue<string>()?.Trim();

            // Validações básicas
            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(tipoStr))
                throw new Exception("Nome, Email e TipoUsuario são obrigatórios");

            // Verificar duplicatas
            if (await _context.Usuarios.AnyAsync(u => u.Email == email))
                throw new Exception($"Email {email} já cadastrado");

            // Criar usuário baseado no tipo
            return await CreateUserByType(nome, email, telefone, tipoStr, matricula, curso);
        }

        private async Task<Usuario> CreateUserByType(string nome, string email, string telefone, 
            string tipoStr, string matricula, string curso)
        {
            var tipo = MapUserType(tipoStr);
            
            switch (tipo)
            {
                case UserType.Admin:
                    return new Admin
                    {
                        Nome = nome,
                        Email = email,
                        Senha = HashPassword("123456"), // Senha padrão
                        NivelAcesso = "Operador",
                        Ativo = true,
                        DataCriacao = DateTime.UtcNow
                    };

                case UserType.Aluno:
                    if (string.IsNullOrEmpty(matricula))
                        throw new Exception("Matrícula é obrigatória para alunos");

                    if (await _context.Alunos.AnyAsync(a => a.Matricula == matricula))
                        throw new Exception($"Matrícula {matricula} já cadastrada");

                    return new Aluno
                    {
                        Nome = nome,
                        Email = email,
                        Senha = HashPassword("123456"),
                        Matricula = matricula,
                        Periodo = "1º",
                        DataNascimento = DateTime.UtcNow.AddYears(-20),
                        Ativo = true,
                        DataCriacao = DateTime.UtcNow
                    };

                case UserType.Professor:
                    return new Professor
                    {
                        Nome = nome,
                        Email = email,
                        Senha = HashPassword("123456"),
                        Formacao = "Graduação",
                        Especialidade = "Geral",
                        Ativo = true,
                        DataCriacao = DateTime.UtcNow
                    };

                case UserType.Coordenador:
                    // Buscar disciplina padrão ou criar
                    var disciplina = await _context.Disciplinas.FirstOrDefaultAsync();
                    if (disciplina == null)
                    {
                        disciplina = new Disciplina { Nome = "Geral", Descricao = "Disciplina padrão" };
                        _context.Disciplinas.Add(disciplina);
                        await _context.SaveChangesAsync();
                    }

                    return new Coordenador
                    {
                        Nome = nome,
                        Email = email,
                        Senha = HashPassword("123456"),
                        AreaCoordenacao = "Geral",
                        DisciplinaId = disciplina.Id,
                        Ativo = true,
                        DataCriacao = DateTime.UtcNow
                    };

                default:
                    throw new Exception($"Tipo de usuário não suportado: {tipoStr}");
            }
        }

        private UserType MapUserType(string tipoStr)
        {
            return tipoStr.ToLower() switch
            {
                "admin" or "administrador" => UserType.Admin,
                "coordenador" => UserType.Coordenador,
                "professor" => UserType.Professor,
                "aluno" => UserType.Aluno,
                _ => throw new Exception($"Tipo de usuário inválido: {tipoStr}. Use: Admin, Coordenador, Professor ou Aluno")
            };
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public int? ImportedCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        
        public void AddError(string error) => Errors.Add(error);
    }

    public enum UserType
    {
        Admin,
        Coordenador,
        Professor,
        Aluno
    }
}