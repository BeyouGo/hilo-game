using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace HiLoGame.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HiLoGameDbContext>
{
    public HiLoGameDbContext CreateDbContext(string[] args)
    {
        var cs = "Server=localhost\\SQLEXPRESS;Database=hilo-db;Trusted_Connection=True;TrustServerCertificate=True";
        var builder = new DbContextOptionsBuilder<HiLoGameDbContext>().UseSqlServer(cs);
        return new HiLoGameDbContext(builder.Options);
    }
}