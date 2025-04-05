using AppEngine.DependencyInjection;
using AppEngine.Partitions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AppEngine.DataAccess;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        var coreExtension = options.FindExtension<Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension>();
        var assemblies = coreExtension?.ApplicationServiceProvider?.GetService<AppAssemblies>();

        foreach (var assembly in assemblies?.Assemblies ?? [])
        {
            builder.ApplyConfigurationsFromAssembly(assembly);
        }

        var basePartition = builder.Model.FindEntityType(typeof(Partition))!;
        var appPartition = builder.Model.GetEntityTypes().Single(met => met.BaseType?.ClrType == typeof(Partition));

        basePartition.SetTableName(appPartition.GetTableName());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>()
                            .HavePrecision(18, 2);
    }
}