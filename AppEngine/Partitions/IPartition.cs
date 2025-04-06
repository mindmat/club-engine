using AppEngine.Authorization.UsersInPartition;
using AppEngine.Authorization.UsersInPartition.AccessRequests;
using AppEngine.Configurations;
using AppEngine.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.Partitions;

public interface IPartition
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Acronym { get; set; }
}

public abstract class Partition : Entity, IPartition
{
    public string Name { get; set; } = null!;
    public string Acronym { get; set; } = null!;
    public ICollection<UserInPartition>? Users { get; set; }
    public ICollection<AccessToPartitionRequest>? AccessRequests { get; set; }
    public ICollection<PartitionConfiguration>? Configurations { get; set; }
}


public class PartitionMap : EntityMap<Partition>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Partition> builder)
    {
        builder.UseTphMappingStrategy();

        builder.Property(ent => ent.Name)
               .HasMaxLength(300);
        builder.Property(ent => ent.Acronym)
               .HasMaxLength(20);
    }
}