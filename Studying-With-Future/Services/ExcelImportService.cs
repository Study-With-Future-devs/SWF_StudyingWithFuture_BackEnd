using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<ImportResult> ImportUsersFromExcel(Stream fileStream)
        {
            var result = new ImportResult();
            var usersToAdd = new List<Usuario>();

            try
            {
                using (var package = new ExcelPackage(fileStream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension?.Rows ?? 0;

                    if (rowCount < 2)
                    {
                        result.AddError("O arquivo está vazio ou não contém dados");
                        return result;
                    }

                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var user = await ParseUserFromRow(worksheet, row);
                            if (user != null)
                            {
                                usersToAdd.Add(user);
                            }
                        }
                        catch (Exception ex)
                        {
                            result.AddError($"Linha {row}: {ex.Message}");
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

        private async Task<Usuario> ParseUserFromRow(ExcelWorksheet worksheet, int row)
        {
            var nome = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
            var email = worksheet.Cells[row, 2].Value?.ToString()?.Trim().ToLower();
            var cpf = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
            var telefone = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
            var tipoStr = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
            var matricula = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
            var curso = worksheet.Cells[row, 7].Value?.ToString()?.Trim();

            // Validações básicas
            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(tipoStr))
                throw new Exception("Nome, Email e TipoUsuario são obrigatórios");

            // Verificar duplicatas
            if (await _context.Usuarios.AnyAsync(u => u.Email == email))
                throw new Exception($"Email {email} já cadastrado");

            if (!string.IsNullOrEmpty(cpf) && await _context.Usuarios.AnyAsync(u => u.CPF == cpf))
                throw new Exception($"CPF {cpf} já cadastrado");

            // Criar usuário baseado no tipo
            return await CreateUserByType(nome, email, cpf, telefone, tipoStr, matricula, curso);
        }

        private async Task<Usuario> CreateUserByType(string nome, string email, string cpf, string telefone, 
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
        public int ImportedCount { get; set; }
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