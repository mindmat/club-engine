using AppEngine.Partitions;

using ClubEngine.ApiService.Members;
using ClubEngine.ApiService.Members.Memberships;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubEngine.ApiService.Clubs;

public class Club : Partition
{
    public ICollection<Member>? Members { get; set; }
    public ICollection<MembershipType>? MembershipTypes { get; set; }
    public ICollection<Period>? Periods { get; set; }
}

public class ClubMap : IEntityTypeConfiguration<Club>
{
    public void Configure(EntityTypeBuilder<Club> builder)
    {
        builder.ToTable("Clubs");
    }
}