using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Studying_With_Future.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Pega a connection string da variável de ambiente
        var connectionString = "server=db;port=3306;database=swf;user=user_swf;password=swf123";

        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new AppDbContext(optionsBuilder.Options);
    }
}
