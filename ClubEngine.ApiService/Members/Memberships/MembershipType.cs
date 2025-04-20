using AppEngine.DataAccess;

using ClubEngine.ApiService.Clubs;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubEngine.ApiService.Members.Memberships;

public class MembershipType : Entity
{
    public Guid ClubId { get; set; }
    public Club? Club { get; set; }
    public string FallbackName { get; set; } = null!;
    public decimal AnnualFee { get; set; }
    public string? Color { get; set; }
}

public class MembershipTypeMap : EntityMap<MembershipType>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MembershipType> builder)
    {
        builder.ToTable("MembershipTypes");

        builder.Property(mst => mst.FallbackName)
               .HasMaxLength(200);

        builder.Property(mst => mst.AnnualFee)
               .HasPrecision(10, 2);

        builder.Property(mst => mst.Color)
               .HasMaxLength(50);

        builder.HasOne(mst => mst.Club)
               .WithMany(clb => clb.MembershipTypes)
               .HasForeignKey(mst => mst.ClubId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}