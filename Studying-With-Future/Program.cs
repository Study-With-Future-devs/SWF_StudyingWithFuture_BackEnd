using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Sistema Escolar API",
        Version = "v1",
        Description = "API para gestão escolar com .NET 9 + Angular"
    });
});

// Primeiro tenta pegar da variável de ambiente, depois do appsettings.json
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
                      ?? builder.Configuration.GetConnectionString("AppDbConnectionString");

if (string.IsNullOrEmpty(connectionString))
{
    // Em desenvolvimento, usa uma string local para testes
    if (builder.Environment.IsDevelopment())
    {
        connectionString = "server=localhost;database=swf;user=root;password=positivo;";
        Console.WriteLine("⚠️  Usando connection string de desenvolvimento");
    }
    else
    {
        throw new Exception("❌ Connection string não configurada. " +
                          "Configure a variável de ambiente DB_CONNECTION_STRING");
    }
}

Console.WriteLine($"✅ Connection String: {connectionString}");

// Configura o DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
           .LogTo(Console.WriteLine, LogLevel.Information) // Log das queries SQL
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

//INICIO LOGICA DE JWT

//FIM LOGICA DE JWT

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Escolar API v1");
        c.RoutePrefix = string.Empty; // Coloca Swagger na raiz (http://localhost:5201)
    });
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Rota de health check
app.MapGet("/health", () => "✅ API está funcionando!");

// Middleware de tratamento de erros global
app.UseExceptionHandler("/error");

// Rota para debug de erros
app.Map("/error", (HttpContext context) =>
{
    var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    return Results.Problem(
        title: exception?.Message,
        detail: exception?.StackTrace,
        statusCode: StatusCodes.Status500InternalServerError
    );
});

Console.WriteLine("🚀 API Iniciando...");
Console.WriteLine($"📊 Swagger: http://localhost:5201");
Console.WriteLine($"🔧 Ambiente: {app.Environment.EnvironmentName}");

app.Run();