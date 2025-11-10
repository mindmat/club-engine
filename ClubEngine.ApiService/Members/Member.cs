using AppEngine.DataAccess;

using ClubEngine.ApiService.Clubs;
using ClubEngine.ApiService.Members.Memberships;
using ClubEngine.ApiService.MembershipFees;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubEngine.ApiService.Members;

public class Member : Entity
{
    public Guid ClubId { get; set; }
    public Club? Club { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Zip { get; set; }
    public string? Town { get; set; }
    public string? Phone { get; set; }
    public List<string> Tags { get; set; } = [];
    public decimal? FeeOverride { get; set; }
    public ICollection<Membership>? Memberships { get; set; }
    public Guid? CurrentMembershipTypeId_ReadModel { get; set; }
    public MembershipType? CurrentMembershipType_ReadModel { get; set; }
    public ICollection<MembershipFee>? Fees { get; set; }
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

        builder.HasOne(mbr => mbr.CurrentMembershipType_ReadModel)
               .WithMany()
               .HasForeignKey(mbr => mbr.CurrentMembershipTypeId_ReadModel);
    }
}