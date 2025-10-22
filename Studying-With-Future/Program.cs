using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using Studying_With_Future.Services;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        // Configurar CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularDev",
                policy =>
                {
                    policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
        });

        // Swagger
        builder.Services.AddEndpointsApiExplorer();

        // Connection string
        var connectionString = "server=db;port=3306;database=swf;user=user_swf;password=swf123";
        Console.WriteLine($"🔍 Tentando conectar com: {connectionString}");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                   .LogTo(Console.WriteLine, LogLevel.Information)
                   .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

        builder.Services.AddScoped<ExcelImportService>();
        builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

        // JWT
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var jwtKey = jwtSettings["Key"] ?? "SuaChaveSecretaSuperSeguraComPeloMenos32Caracteres123!";

        if (jwtKey.Length < 32)
            throw new Exception("A chave JWT deve ter pelo menos 32 caracteres");

        var key = Encoding.UTF8.GetBytes(jwtKey);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"] ?? "SWF_API",
                ValidAudience = jwtSettings["Audience"] ?? "SWF_Client",
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"❌ Autenticação falhou: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine($"✅ Token validado para: {context.Principal?.Identity?.Name ?? "Unknown"}");
                    return Task.CompletedTask;
                }
            };
        });

        // Authorization policies
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("Coordenador", policy => policy.RequireRole("Coordenador", "Admin"));
            options.AddPolicy("Professor", policy => policy.RequireRole("Professor", "Coordenador", "Admin"));
            options.AddPolicy("Aluno", policy => policy.RequireRole("Aluno", "Professor", "Coordenador", "Admin"));
            options.AddPolicy("Usuário", policy => policy.RequireRole("Usuário", "Aluno", "Professor", "Coordenador", "Admin"));
            options.AddPolicy("AnyAuthenticated", policy => policy.RequireAuthenticatedUser());
        });

        // Swagger configuration
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Sistema Escolar API",
                Version = "v1",
                Description = "API para gestão escolar com .NET 9 + Angular"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        var app = builder.Build();

        // Pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Escolar API v1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseCors("AllowAngularDev");
        //app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // Health check
        app.MapGet("/health", () => "✅ API está funcionando!");

        // Middleware global de erros
        app.UseExceptionHandler("/error");
        app.Map("/error", (HttpContext context) =>
        {
            var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
            return Results.Problem(
                title: exception?.Message,
                detail: exception?.StackTrace,
                statusCode: StatusCodes.Status500InternalServerError
            );
        });

        // Porta fixa
        app.Urls.Add("http://*:5004");

        Console.WriteLine("🚀 API Iniciando...");
        Console.WriteLine($"📊 Swagger: http://localhost:5004");
        Console.WriteLine($"🔧 Ambiente: {app.Environment.EnvironmentName}");

        await app.RunAsync();
    }
}
