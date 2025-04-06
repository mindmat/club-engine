using AppEngine.DataAccess;
using AppEngine.Partitions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.DomainEvents;

public class PersistedDomainEvent : Entity
{
    public Guid? PartitionId { get; set; }
    public Partition? Partition { get; set; }

    public string Type { get; set; } = null!;
    public string Data { get; set; } = null!;
    public Guid? DomainEventId_Parent { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class PersistedDomainEventMap : EntityMap<PersistedDomainEvent>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PersistedDomainEvent> builder)
    {
        builder.ToTable("DomainEvents");

        builder.HasOne(dev => dev.Partition)
               .WithMany()
               .HasForeignKey(dev => dev.PartitionId);

        builder.Property(dev => dev.Type)
               .HasMaxLength(300);

        builder.HasIndex(dev => dev.Timestamp)
               .IsUnique(false);
    }
}