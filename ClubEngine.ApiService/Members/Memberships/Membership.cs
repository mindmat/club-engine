using AppEngine.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubEngine.ApiService.Members.Memberships;

public class Membership : Entity
{
    public Guid MemberId { get; set; }
    public Member? Member { get; set; }
    public Guid MembershipTypeId { get; set; }
    public MembershipType? MembershipType { get; set; }
    public DateOnly From { get; set; }
    public DateOnly Until { get; set; }
}

public class MembershipMap : EntityMap<Membership>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("Memberships");

        builder.HasOne(mbr => mbr.Member)
               .WithMany(mbr => mbr.Memberships)
               .HasForeignKey(mbr => mbr.MemberId);
    }
}