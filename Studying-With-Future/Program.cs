using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

internal class Program
{
    private static void Main(string[] args)
    {
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
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var jwtKey = jwtSettings["Key"] ?? "SuaChaveSecretaSuperSeguraComPeloMenos32Caracteres123!";

        if (jwtKey.Length < 32)
        {
            throw new Exception("A chave JWT deve ter pelo menos 32 caracteres");
        }

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
                ClockSkew = TimeSpan.Zero // Remove tolerância de tempo
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
                    Console.WriteLine($"✅ Token validado para: {context.Principal.Identity.Name}");
                    return Task.CompletedTask;
                }
            };
        });

        // Configurar políticas de autorização
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy("Coordenador", policy =>
                policy.RequireRole("Coordenador", "Admin"));

            options.AddPolicy("Professor", policy =>
                policy.RequireRole("Professor", "Coordenador", "Admin"));

            options.AddPolicy("Aluno", policy =>
                policy.RequireRole("Aluno", "Professor", "Coordenador", "Admin"));

            options.AddPolicy("Usuário", policy =>
                policy.RequireRole("Usuário", "Aluno", "Professor", "Coordenador", "Admin"));

            options.AddPolicy("AnyAuthenticated", policy =>
                policy.RequireAuthenticatedUser());

            // Política específica para acesso a telas
            options.AddPolicy("TelaAccess", policy =>
                policy.RequireAssertion(context =>
                {
                    // Esta política será verificada manualmente no middleware
                    return true;
                }));
        });

        // Configurar Swagger com suporte a JWT
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Sistema Escolar API",
                Version = "v1",
                Description = "API para gestão escolar com .NET 9 + Angular"
            });

            // Adicionar suporte a JWT no Swagger
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
        }); // FIM AddSwaggerGen

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
        app.UseAuthentication(); // Added missing authentication middleware
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
    }
}