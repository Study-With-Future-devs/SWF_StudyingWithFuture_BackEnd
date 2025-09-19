using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Studying_With_Future.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Pega a connection string da variável de ambiente
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                               ?? "server=mysql_db;database=swf;user=swf_user;password=swf_password;";

        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new AppDbContext(optionsBuilder.Options);
    }
}
