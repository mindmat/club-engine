using AppEngine.Partitions;

using ClubEngine.ApiService.Members;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubEngine.ApiService.Clubs;

public class Club : Partition
{
    public ICollection<Member>? Members { get; set; }
}

public class ClubMap : IEntityTypeConfiguration<Club>
{
    public void Configure(EntityTypeBuilder<Club> builder)
    {
        builder.ToTable("Clubs");
    }
}