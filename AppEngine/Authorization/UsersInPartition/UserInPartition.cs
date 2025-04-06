using AppEngine.Authentication.Users;
using AppEngine.DataAccess;
using AppEngine.Partitions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.Authorization.UsersInPartition;

public class UserInPartition : Entity
{
    public Partition? Partition { get; set; }
    public Guid PartitionId { get; set; }
    public User? User { get; set; }
    public Guid UserId { get; set; }

    public UserInPartitionRole Role { get; set; }
}

public class UserInPartitionMap : EntityMap<UserInPartition>
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserInPartition> builder)
    {
        builder.ToTable("UsersInPartitions");

        builder.HasOne(uie => uie.Partition)
               .WithMany(evt => evt.Users)
               .HasForeignKey(uie => uie.PartitionId);

        builder.HasOne(uie => uie.User)
               .WithMany(evt => evt.Partitions)
               .HasForeignKey(uie => uie.UserId);
    }
}