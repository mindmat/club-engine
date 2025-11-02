using AppEngine.DataAccess;

using ClubEngine.ApiService.Clubs;
using ClubEngine.ApiService.Members;
using ClubEngine.ApiService.Members.Memberships;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubEngine.ApiService.MembershipFees;

public class MembershipFee : Entity
{
    public Guid PeriodId { get; set; }
    public Period? Period { get; set; }
    public Guid MemberId { get; set; }
    public Member? Member { get; set; }
    public Guid MembershipTypeId { get; set; }
    public MembershipType? MembershipType { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountPaid_ReadModel { get; set; }
    public MembershipFeeState State { get; set; }
}

public enum MembershipFeeState
{
    Due       = 1,
    Paid      = 2,
    Cancelled = 3
}

public class MembershipFeeMap : EntityMap<MembershipFee>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MembershipFee> builder)
    {
        builder.ToTable("MembershipFees");

        builder.HasOne(mfe => mfe.Period)
               .WithMany(per => per.Fees)
               .HasForeignKey(mfe => mfe.PeriodId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mfe => mfe.Member)
               .WithMany(mbr => mbr.Fees)
               .HasForeignKey(mfe => mfe.MemberId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mfe => mfe.MembershipType)
               .WithMany()
               .HasForeignKey(mfe => mfe.MembershipTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}