using AppEngine.DataAccess;

using ClubEngine.ApiService.Clubs;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubEngine.ApiService.Members;

public class Member : Entity
{
    public Guid ClubId { get; set; }
    public Club? Club { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}

public class MemberMap : EntityMap<Member>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        builder.Property(ent => ent.FirstName)
               .HasMaxLength(200);

        builder.Property(ent => ent.LastName)
               .HasMaxLength(200);

        builder.Property(ent => ent.Email)
               .HasMaxLength(500);

        builder.HasOne(mbr => mbr.Club)
               .WithMany(clb => clb.Members)
               .HasForeignKey(mbr => mbr.ClubId);
    }
}