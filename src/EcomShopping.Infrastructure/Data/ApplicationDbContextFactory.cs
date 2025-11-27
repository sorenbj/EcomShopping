using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EcomShopping.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Use SQL Server for migrations (design time only)
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EcomShoppingDb;Trusted_Connection=True;MultipleActiveResultSets=true");
        
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
