using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Studying_With_Future.Services;

namespace Studying_With_Future.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ImportacaoController : ControllerBase
    {
        private readonly ExcelImportService _excelService;
        private readonly IWebHostEnvironment _environment;

        public ImportacaoController(ExcelImportService excelService, IWebHostEnvironment environment)
        {
            _excelService = excelService;
            _environment = environment;
        }

        [HttpPost("usuarios")]
        public async Task<ActionResult<ApiResponse<ImportResult>>> ImportarUsuarios(IFormFile arquivo)
        {
            try
            {
                // Validações do arquivo
                if (arquivo == null || arquivo.Length == 0)
                    return BadRequest(new ApiResponse<object>(false, "Nenhum arquivo selecionado"));

                if (!Path.GetExtension(arquivo.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponse<object>(false, "Formato inválido. Use arquivo .xlsx"));

                if (arquivo.Length > 5 * 1024 * 1024) // 5MB
                    return BadRequest(new ApiResponse<object>(false, "Arquivo muito grande. Máximo 5MB"));

                using (var stream = arquivo.OpenReadStream())
                {
                    var resultado = await _excelService.ImportUsersFromExcel(stream);
                    
                    if (resultado.Success)
                    {
                        return Ok(new ApiResponse<ImportResult>(true, 
                            $"✅ Importação concluída: {resultado.ImportedCount} usuários importados com sucesso", 
                            resultado));
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<ImportResult>(false, 
                            "❌ Erros encontrados na importação", 
                            resultado));
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Erro interno: {ex.Message}"));
            }
        }

        [HttpGet("template-usuarios")]
        public IActionResult DownloadTemplate()
        {
            try
            {
                // Criar template dinamicamente
                return CreateDynamicTemplate();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Erro ao gerar template: {ex.Message}"));
            }
        }

        private IActionResult CreateDynamicTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Usuarios");
                
                // Cabeçalhos
                worksheet.Cells[1, 1].Value = "Nome";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "CPF";
                worksheet.Cells[1, 4].Value = "Telefone";
                worksheet.Cells[1, 5].Value = "TipoUsuario";
                worksheet.Cells[1, 6].Value = "Matricula";
                worksheet.Cells[1, 7].Value = "Curso";
                
                // Instruções
                worksheet.Cells[2, 1].Value = "João Silva";
                worksheet.Cells[2, 2].Value = "joao.silva@email.com";
                worksheet.Cells[2, 3].Value = "123.456.789-00";
                worksheet.Cells[2, 4].Value = "(11) 99999-9999";
                worksheet.Cells[2, 5].Value = "Aluno";
                worksheet.Cells[2, 6].Value = "20230001";
                worksheet.Cells[2, 7].Value = "Engenharia";
                
                worksheet.Cells[3, 1].Value = "Maria Santos";
                worksheet.Cells[3, 2].Value = "maria.santos@email.com";
                worksheet.Cells[3, 3].Value = "987.654.321-00";
                worksheet.Cells[3, 4].Value = "(11) 98888-8888";
                worksheet.Cells[3, 5].Value = "Professor";
                worksheet.Cells[3, 6].Value = "";
                worksheet.Cells[3, 7].Value = "Matemática";
                
                worksheet.Cells[4, 1].Value = "Carlos Oliveira";
                worksheet.Cells[4, 2].Value = "carlos.oliveira@email.com";
                worksheet.Cells[4, 3].Value = "456.789.123-00";
                worksheet.Cells[4, 4].Value = "(11) 97777-7777";
                worksheet.Cells[4, 5].Value = "Coordenador";
                worksheet.Cells[4, 6].Value = "";
                worksheet.Cells[4, 7].Value = "Administração";
                
                worksheet.Cells[5, 1].Value = "Admin Sistema";
                worksheet.Cells[5, 2].Value = "admin@escola.com";
                worksheet.Cells[5, 3].Value = "111.222.333-44";
                worksheet.Cells[5, 4].Value = "(11) 96666-6666";
                worksheet.Cells[5, 5].Value = "Admin";
                worksheet.Cells[5, 6].Value = "";
                worksheet.Cells[5, 7].Value = "";
                
                // Formatação
                worksheet.Cells[1, 1, 1, 7].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 5, 7].AutoFitColumns();
                
                // Adicionar instruções
                worksheet.Cells[7, 1].Value = "INSTRUÇÕES:";
                worksheet.Cells[8, 1].Value = "- Campos obrigatórios: Nome, Email, TipoUsuario";
                worksheet.Cells[9, 1].Value = "- TipoUsuario deve ser: Admin, Coordenador, Professor ou Aluno";
                worksheet.Cells[10, 1].Value = "- Matricula é obrigatória apenas para Alunos";
                worksheet.Cells[11, 1].Value = "- Todos os usuários criados terão senha padrão: 123456";
                worksheet.Cells[12, 1].Value = "- CPF e Telefone são opcionais";
                
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "template-cadastro-usuarios.xlsx");
            }
        }
    }

    // Model para resposta padrão da API
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiResponse(bool success, string message, T data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}