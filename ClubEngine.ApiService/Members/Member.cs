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
    public MembershipType MembershipType { get; set; }
    public string? Address { get; set; }
    public string? Zip { get; set; }
    public string? Town { get; set; }
    public string? Phone { get; set; }
    public List<string> Tags { get; set; } = new();
    public required DateOnly MemberFrom { get; set; }
    public required DateOnly MemberUntil { get; set; }
}

public enum MembershipType
{
    Active   = 1,
    Passive  = 2,
    Honorary = 3
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

        builder.Property(ent => ent.Address)
               .HasMaxLength(500);

        builder.Property(ent => ent.Zip)
               .HasMaxLength(20);

        builder.Property(ent => ent.Town)
               .HasMaxLength(200);

        builder.Property(ent => ent.Phone)
               .HasMaxLength(100);


        builder.HasOne(mbr => mbr.Club)
               .WithMany(clb => clb.Members)
               .HasForeignKey(mbr => mbr.ClubId);
    }
}