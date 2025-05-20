using AppEngine.DataAccess;

using ClubEngine.ApiService.Members;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubEngine.ApiService.Slack;

public class SlackUserMapping : Entity
{
    public Guid MemberId { get; set; }
    public Member? Member { get; set; }
    public required string SlackUserId { get; set; }
}

public class SlackUserMappingMap : EntityMap<SlackUserMapping>
{
    protected override void ConfigureEntity(EntityTypeBuilder<SlackUserMapping> builder)
    {
        builder.HasOne(sum => sum.Member)
               .WithMany()
               .HasForeignKey(sum => sum.MemberId);

        builder.Property(sum => sum.SlackUserId)
               .HasMaxLength(100);
    }
}