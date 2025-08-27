using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Primeiro tenta pegar da variável de ambiente, depois do appsettings.json
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
                            ?? builder.Configuration.GetConnectionString("AppDbConnectionString");

        // Verifica se a connection string está configurada
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("❌ Connection string não configurada. " +
                            "Configure a variável de ambiente DB_CONNECTION_STRING");
        }

        Console.WriteLine($"✅ Connection String: {connectionString}");

        // Configura o DbContext
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}