using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Studying_With_Future.Data;

namespace Studying_With_Future.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Connection string para design time - use localhost:3305
            var connectionString = "server=localhost;port=3305;database=swf;user=user_swf;password=swf123";
            
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}