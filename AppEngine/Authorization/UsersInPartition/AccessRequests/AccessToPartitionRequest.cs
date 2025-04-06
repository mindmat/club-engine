using AppEngine.Authentication;
using AppEngine.Authentication.Users;
using AppEngine.DataAccess;
using AppEngine.Partitions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.Authorization.UsersInPartition.AccessRequests;

public class AccessToPartitionRequest : Entity
{
    public Guid PartitionId { get; set; }
    public Partition? Partition { get; set; }
    public Guid? UserId_Requestor { get; set; }
    public User? User_Requestor { get; set; }
    public Guid? UserId_Responder { get; set; }
    public User? User_Responder { get; set; }

    public string? Identifier { get; set; }
    public IdentityProvider IdentityProvider { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }

    public DateTimeOffset RequestReceived { get; set; }
    public string? RequestText { get; set; }
    public RequestResponse? Response { get; set; }
    public string? ResponseText { get; set; }
}

public class AccessToEventRequestMap : EntityMap<AccessToPartitionRequest>
{
    protected override void ConfigureEntity(EntityTypeBuilder<AccessToPartitionRequest> builder)
    {
        builder.ToTable("AccessToPartitionsRequests");

        builder.HasOne(arq => arq.Partition)
               .WithMany(evt => evt.AccessRequests)
               .HasForeignKey(arq => arq.PartitionId);

        builder.HasOne(arq => arq.User_Requestor)
               .WithMany()
               .HasForeignKey(arq => arq.UserId_Requestor);

        builder.HasOne(arq => arq.User_Responder)
               .WithMany()
               .HasForeignKey(arq => arq.UserId_Responder);

        builder.Property(arq => arq.Identifier)
               .HasMaxLength(200);
        builder.Property(arq => arq.FirstName)
               .HasMaxLength(200);
        builder.Property(arq => arq.LastName)
               .HasMaxLength(200);
        builder.Property(arq => arq.Email)
               .HasMaxLength(200);
        builder.Property(arq => arq.AvatarUrl)
               .HasMaxLength(500);
    }
}