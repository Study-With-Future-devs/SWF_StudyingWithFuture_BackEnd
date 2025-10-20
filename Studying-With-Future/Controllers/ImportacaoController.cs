using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Studying_With_Future.Services;
using ClosedXML.Excel;

namespace Studying_With_Future.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Roles = "Admin")]
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
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Usuarios");
                
                // Cabeçalhos
                worksheet.Cell(1, 1).Value = "Nome";
                worksheet.Cell(1, 2).Value = "Email";
                worksheet.Cell(1, 3).Value = "CPF";
                worksheet.Cell(1, 4).Value = "Telefone";
                worksheet.Cell(1, 5).Value = "TipoUsuario";
                worksheet.Cell(1, 6).Value = "Matricula";
                worksheet.Cell(1, 7).Value = "Curso";
                
                // Dados de exemplo
                worksheet.Cell(2, 1).Value = "João Silva";
                worksheet.Cell(2, 2).Value = "joao.silva@email.com";
                worksheet.Cell(2, 3).Value = "123.456.789-00";
                worksheet.Cell(2, 4).Value = "(11) 99999-9999";
                worksheet.Cell(2, 5).Value = "Aluno";
                worksheet.Cell(2, 6).Value = "20230001";
                worksheet.Cell(2, 7).Value = "Engenharia";
                
                worksheet.Cell(3, 1).Value = "Maria Santos";
                worksheet.Cell(3, 2).Value = "maria.santos@email.com";
                worksheet.Cell(3, 3).Value = "987.654.321-00";
                worksheet.Cell(3, 4).Value = "(11) 98888-8888";
                worksheet.Cell(3, 5).Value = "Professor";
                worksheet.Cell(3, 6).Value = "";
                worksheet.Cell(3, 7).Value = "Matemática";
                
                worksheet.Cell(4, 1).Value = "Carlos Oliveira";
                worksheet.Cell(4, 2).Value = "carlos.oliveira@email.com";
                worksheet.Cell(4, 3).Value = "456.789.123-00";
                worksheet.Cell(4, 4).Value = "(11) 97777-7777";
                worksheet.Cell(4, 5).Value = "Coordenador";
                worksheet.Cell(4, 6).Value = "";
                worksheet.Cell(4, 7).Value = "Administração";
                
                worksheet.Cell(5, 1).Value = "Admin Sistema";
                worksheet.Cell(5, 2).Value = "admin@escola.com";
                worksheet.Cell(5, 3).Value = "111.222.333-44";
                worksheet.Cell(5, 4).Value = "(11) 96666-6666";
                worksheet.Cell(5, 5).Value = "Admin";
                worksheet.Cell(5, 6).Value = "";
                worksheet.Cell(5, 7).Value = "";
                
                // Formatação
                var headerRange = worksheet.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                
                worksheet.Columns().AdjustToContents();
                
                // Adicionar instruções
                worksheet.Cell(7, 1).Value = "INSTRUÇÕES:";
                worksheet.Cell(8, 1).Value = "- Campos obrigatórios: Nome, Email, TipoUsuario";
                worksheet.Cell(9, 1).Value = "- TipoUsuario deve ser: Admin, Coordenador, Professor ou Aluno";
                worksheet.Cell(10, 1).Value = "- Matricula é obrigatória apenas para Alunos";
                worksheet.Cell(11, 1).Value = "- Todos os usuários criados terão senha padrão: 123456";
                worksheet.Cell(12, 1).Value = "- CPF e Telefone são opcionais";
                
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream, 
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        "template-cadastro-usuarios.xlsx");
                }
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