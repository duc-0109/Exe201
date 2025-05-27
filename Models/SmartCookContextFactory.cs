using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace SmartCookFinal.Models
{
    public class SmartCookContextFactory : IDesignTimeDbContextFactory<SmartCookContext>
    {
        public SmartCookContext CreateDbContext(string[] args)
        {
            // Đọc cấu hình từ appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Thư mục chạy EF
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<SmartCookContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString); // Hoặc UseNpgsql, UseSqlite...

            return new SmartCookContext(builder.Options);
        }
    }
}
