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

        // Configurar CORS primeiro
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

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddEndpointsApiExplorer();

        // Connection string (aqui já está fixada para o container)
        var connectionString = "server=db;port=3306;database=swf;user=user_swf;password=swf123";

        Console.WriteLine($"🔍 Tentando conectar com: {connectionString}");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                   .LogTo(Console.WriteLine, LogLevel.Information)
                   .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

        builder.Services.AddScoped<ExcelImportService>();
        builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

        // INICIO LOGICA DE JWT
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

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("Coordenador", policy => policy.RequireRole("Coordenador", "Admin"));
            options.AddPolicy("Professor", policy => policy.RequireRole("Professor", "Coordenador", "Admin"));
            options.AddPolicy("Aluno", policy => policy.RequireRole("Aluno", "Professor", "Coordenador", "Admin"));
            options.AddPolicy("Usuário", policy => policy.RequireRole("Usuário", "Aluno", "Professor", "Coordenador", "Admin"));
            options.AddPolicy("AnyAuthenticated", policy => policy.RequireAuthenticatedUser());
        });

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

        // 🛡️ MIGRATIONS AUTOMÁTICAS - VERSÃO 100% CONFIÁVEL
        await ApplyMigrationsSafely(app);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Escolar API v1");
                c.RoutePrefix = string.Empty;
            });
        }

        // IMPORTANTE: UseCors deve vir antes de UseAuthentication e UseAuthorization
        app.UseCors("AllowAngularDev");

        //app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Rota de health check
        app.MapGet("/health", () => "✅ API está funcionando!");

        // Middleware de tratamento de erros global
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

        // 🚀 Forçar a API a escutar na porta 5004
        app.Urls.Add("http://*:5004");

        Console.WriteLine("🚀 API Iniciando...");
        Console.WriteLine($"📊 Swagger: http://localhost:5004");
        Console.WriteLine($"🔧 Ambiente: {app.Environment.EnvironmentName}");

        await app.RunAsync();
    }

    /// <summary>
    /// 🛡️ Aplica migrations de forma segura sem crashar a API
    /// </summary>
    private static async Task ApplyMigrationsSafely(WebApplication app)
    {
        const int maxRetries = 3;
        const int retryDelayMs = 2000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var services = scope.ServiceProvider;
                
                var context = services.GetRequiredService<AppDbContext>();
                var logger = services.GetRequiredService<ILogger<Program>>();

                Console.WriteLine($"🔄 Tentativa {attempt}/{maxRetries}: Verificando conexão com o banco...");

                // 1. Verificar conexão com timeout
                var canConnect = await TryConnectWithTimeout(context, TimeSpan.FromSeconds(10));
                if (!canConnect)
                {
                    logger.LogWarning("❌ Não foi possível conectar ao banco de dados");
                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"⏳ Aguardando {retryDelayMs}ms antes da próxima tentativa...");
                        await Task.Delay(retryDelayMs);
                        continue;
                    }
                    else
                    {
                        logger.LogError("🚫 Todas as tentativas de conexão falharam. Continuando sem migrations...");
                        return;
                    }
                }

                Console.WriteLine("✅ Conexão com o banco estabelecida!");

                // 2. Verificar migrations pendentes
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (!pendingMigrations.Any())
                {
                    Console.WriteLine("✅ Nenhuma migration pendente.");
                    return;
                }

                Console.WriteLine($"📦 Encontradas {pendingMigrations.Count()} migrations pendentes:");
                foreach (var migration in pendingMigrations)
                {
                    Console.WriteLine($"   - {migration}");
                }

                // 3. Aplicar migrations com transaction
                Console.WriteLine("🔧 Aplicando migrations...");
                await context.Database.MigrateAsync();
                
                Console.WriteLine("✅ Todas as migrations aplicadas com sucesso!");
                return; // Sucesso - sai do método
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                
                if (attempt == maxRetries)
                {
                    // Última tentativa - loga como erro mas não crasha
                    logger.LogError(ex, "🚫 ERRO CRÍTICO: Falha ao aplicar migrations após {Attempt} tentativas", attempt);
                    Console.WriteLine("⚠️ AVISO: Migrations não aplicadas. A API continuará funcionando.");
                    Console.WriteLine("💡 SOLUÇÃO: Aplique as migrations manualmente com: dotnet ef database update");
                }
                else
                {
                    // Tentativas intermediárias - loga como warning
                    logger.LogWarning(ex, "⚠️ Tentativa {Attempt}/{MaxRetries} falhou", attempt, maxRetries);
                    Console.WriteLine($"⏳ Aguardando {retryDelayMs}ms antes da próxima tentativa...");
                    await Task.Delay(retryDelayMs);
                }
            }
        }
    }

    /// <summary>
    /// 🕐 Tenta conectar ao banco com timeout
    /// </summary>
    private static async Task<bool> TryConnectWithTimeout(AppDbContext context, TimeSpan timeout)
    {
        try
        {
            var cts = new CancellationTokenSource(timeout);
            return await context.Database.CanConnectAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("⏰ Timeout ao conectar com o banco");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔌 Erro de conexão: {ex.Message}");
            return false;
        }
    }
}