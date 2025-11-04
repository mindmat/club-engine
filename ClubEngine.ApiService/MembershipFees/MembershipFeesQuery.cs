using System.Linq;

using AppEngine.Authorization;
using AppEngine.DataAccess;
using AppEngine.TimeHandling;

using ClubEngine.ApiService.Clubs;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.MembershipFees;

public record MembershipFeesQuery(Guid? PeriodId) : IRequest<MembershipFeesList>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
}

public record MembershipFeesList(IEnumerable<FeeStateInPeriod> Paid,
                                 IEnumerable<FeeStateInPeriod> Due,
                                 decimal SumPaid,
                                 decimal SumDue);

public record FeeStateInPeriod(Guid MemberId, string MemberName, decimal AmountExpected, decimal AmountPaid, MembershipFeeState State);

public class MembershipFeesQueryHandler(IQueryable<MembershipFee> fees,
                                        IQueryable<Period> periods,
                                        RequestTimeProvider timeProvider) : IRequestHandler<MembershipFeesQuery, MembershipFeesList>
{
    public async Task<MembershipFeesList> Handle(MembershipFeesQuery query, CancellationToken cancellationToken)
    {
        var period = periods.Where(per => per.ClubId == query.PartitionId)
                            .WhereIf(query.PeriodId != null, per => per.Id == query.PeriodId)
                            .WhereIf(query.PeriodId == null,
                                     per => per.From <= timeProvider.RequestToday
                                         && timeProvider.RequestToday <= per.Until)
                            .FirstOrDefault()
                  ?? throw new ArgumentNullException(nameof(query.PeriodId));

        var feesOfPeriod = await fees.Where(mfe => mfe.PeriodId == period.Id)
                                     .OrderBy(mbr => mbr.Member!.FirstName)
                                     .ThenBy(mbr => mbr.Member!.LastName)
                                     .Select(mfe => new FeeStateInPeriod(mfe.MemberId,
                                                                         $"{mfe.Member!.FirstName} {mfe.Member.LastName}",
                                                                         mfe.Amount,
                                                                         mfe.AmountPaid_ReadModel,
                                                                         mfe.State))
                                     .ToListAsync(cancellationToken);

        var paidFees = feesOfPeriod.Where(fee => fee.State == MembershipFeeState.Paid)
                                   .ToList();

        var dueFees = feesOfPeriod.Where(fee => fee.State == MembershipFeeState.Due)
                                  .ToList();

        return new MembershipFeesList(paidFees,
                                      dueFees,
                                      paidFees.Sum(fee => fee.AmountPaid),
                                      dueFees.Sum(fee => fee.AmountExpected - fee.AmountPaid));
    }
}