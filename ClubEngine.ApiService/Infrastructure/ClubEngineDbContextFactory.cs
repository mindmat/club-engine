using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClubEngine.ApiService.Infrastructure;

public class ClubEngineDbContextFactory : IDesignTimeDbContextFactory<ClubEngineDbContext>
{
    public ClubEngineDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClubEngineDbContext>().UseSqlServer()
                                                                               .EnableSensitiveDataLogging();

        return new ClubEngineDbContext(optionsBuilder.Options);
    }
}