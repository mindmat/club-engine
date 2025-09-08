using AppEngine.DataAccess;
using AppEngine.Partitions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.Configurations;

public class PartitionConfiguration : Entity
{
    public Partition? Partition { get; set; }
    public Guid PartitionId { get; set; }

    public string Type { get; set; } = null!;
    public string ValueJson { get; set; } = null!;
}

public class PartitionConfigurationMap : EntityMap<PartitionConfiguration>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PartitionConfiguration> builder)
    {
        builder.ToTable("PartitionConfigurations");

        builder.HasOne(cfg => cfg.Partition)
               .WithMany(evt => evt.Configurations)
               .HasForeignKey(cfg => cfg.PartitionId);

        builder.Property(cfg => cfg.Type)
               .HasMaxLength(300);
    }
}