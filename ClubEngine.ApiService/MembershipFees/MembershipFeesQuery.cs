using AppEngine.Authorization;
using AppEngine.DataAccess;
using AppEngine.TimeHandling;

using ClubEngine.ApiService.Clubs;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.MembershipFees;

public record MembershipFeesQuery(Guid? PeriodId) : IRequest<MembershipFeesList>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
}

public record MembershipFeesList(IEnumerable<FeeStateInPeriod> Paid,
                                 IEnumerable<FeeStateInPeriod> Due);

public record FeeStateInPeriod(Guid MemberId, string MemberName, decimal AmountExpected, decimal AmountPaid);

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
                                     .Select(mfe => new
                                                    {
                                                        mfe.MemberId,
                                                        Member = $"{mfe.Member!.FirstName} {mfe.Member.LastName}",
                                                        mfe.Amount,
                                                        mfe.State
                                                    })
                                     .ToListAsync(cancellationToken);

        return new MembershipFeesList(feesOfPeriod.Where(fee => fee.State == MembershipFeeState.Paid)
                                                  .Select(fee => new FeeStateInPeriod(fee.MemberId, fee.Member, fee.Amount, 0))
                                                  .ToList(),
                                      feesOfPeriod.Where(fee => fee.State == MembershipFeeState.Due)
                                                  .Select(fee => new FeeStateInPeriod(fee.MemberId, fee.Member, fee.Amount, 0))
                                                  .ToList());
    }
}