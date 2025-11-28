using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EcomShopping.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Use SQL Server Express for migrations (design time only)
        optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=EcomShoppingDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
        
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
