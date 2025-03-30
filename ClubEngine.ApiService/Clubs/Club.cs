using AppEngine.DataAccess;
using ClubEngine.ApiService.Members;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubEngine.ApiService.Clubs;

public class Club : Entity
{
    public string Name { get; set; } = null!;
    public ICollection<Member>? Members { get; set; }
}

public class ClubMap : EntityMap<Club>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Club> builder)
    {
        builder.ToTable("Clubs");

        builder.Property(ent => ent.Name)
               .HasMaxLength(300);
    }
}