using AppEngine.DataAccess;
using AppEngine.TimeHandling;

using ClubEngine.ApiService.MembershipFees;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubEngine.ApiService.Clubs;

public class Period : Entity, IDatePeriod
{
    public Guid ClubId { get; set; }
    public Club? Club { get; set; }
    public DateOnly From { get; set; }
    public DateOnly Until { get; set; }
    public ICollection<MembershipFee>? Fees { get; set; }
}

public class PeriodMap : EntityMap<Period>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Period> builder)
    {
        builder.ToTable("Periods");

        builder.HasOne(per => per.Club)
               .WithMany(club => club.Periods)
               .HasForeignKey(p => p.ClubId);
    }
}