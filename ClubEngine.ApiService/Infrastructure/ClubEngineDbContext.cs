using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.Infrastructure;

public class ClubEngineDbContext(DbContextOptions<ClubEngineDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ClubEngineDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>()
                            .HavePrecision(18, 2);
    }
}