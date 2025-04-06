using AppEngine.DataAccess;
using AppEngine.Json;
using AppEngine.Partitions;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Configurations;

public class ConfigurationRegistry(IEnumerable<IDefaultConfigurationItem> defaultConfigurations,
                                   IRepository<PartitionConfiguration> configurations,
                                   Serializer serializer,
                                   PartitionContext eventContext)
{
    public T GetConfiguration<T>(Guid? partitionId = null)
        where T : class, IConfigurationItem
    {
        partitionId ??= eventContext.PartitionId;
        if (partitionId != null)
        {
            var dbConfig = configurations.FirstOrDefault(cfg => cfg.PartitionId == partitionId.Value
                                                             && cfg.Type == typeof(T).FullName);
            if (dbConfig != null)
            {
                return serializer.Deserialize<T>(dbConfig.ValueJson)!;
            }
        }

        var defaultConfig = defaultConfigurations
            .FirstOrDefault(dfc => dfc.GetType().BaseType == typeof(T));
        return defaultConfig as T;
    }

    public async Task UpdateConfiguration<T>(Guid partitionId, T newConfig)
        where T : class, IConfigurationItem
    {
        var dbConfig = await configurations.AsTracking()
                                           .FirstOrDefaultAsync(cfg => cfg.PartitionId == partitionId
                                                                    && cfg.Type == typeof(T).FullName)
                    ?? configurations.Insert(new PartitionConfiguration
                                                       {
                                                           Id = Guid.NewGuid(),
                                                           PartitionId = partitionId,
                                                           Type = typeof(T).FullName!
                                                       });
        dbConfig.ValueJson = serializer.Serialize(newConfig);
    }

    public IConfigurationItem GetConfigurationTypeless(Type type)
    {
        return typeof(ConfigurationRegistry).GetMethod(nameof(GetConfiguration))!
                                            .MakeGenericMethod(type)
                                            .Invoke(this, [null]) as IConfigurationItem;
    }
}