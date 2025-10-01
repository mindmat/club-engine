using AppEngine.Authorization;
using AppEngine.DataAccess;
using AppEngine.TimeHandling;

using ClubEngine.ApiService.Clubs;
using ClubEngine.ApiService.Members.Memberships;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.MembershipFees;

public class UpsertMembershipFeesForPeriodCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public Guid? PeriodId { get; set; }
}

public class UpsertMembershipFeesForPeriodCommandHandler(IQueryable<Membership> memberships,
                                                         IQueryable<Period> periods,
                                                         IRepository<MembershipFee> fees,
                                                         RequestTimeProvider timeProvider)
    : IRequestHandler<UpsertMembershipFeesForPeriodCommand>
{
    public async Task Handle(UpsertMembershipFeesForPeriodCommand command, CancellationToken cancellationToken)
    {
        var period = periods.Where(per => per.ClubId == command.PartitionId)
                            .WhereIf(command.PeriodId != null, per => per.Id == command.PeriodId)
                            .WhereIf(command.PeriodId == null,
                                     per => per.From <= timeProvider.RequestToday
                                         && timeProvider.RequestToday <= per.Until)
                            .FirstOrDefault()
                  ?? throw new ArgumentNullException(nameof(command.PeriodId));

        var activeMembers = await memberships.Where(mbr => mbr.Member!.ClubId == command.PartitionId)
                                             .Overlaps(period)
                                             .Select(mbr => new
                                                            {
                                                                mbr.MemberId,
                                                                mbr.MembershipTypeId,
                                                                Fee = mbr.AnnualFeeOverride ?? mbr.MembershipType!.AnnualFee,
                                                                mbr.From,
                                                                mbr.Until
                                                            })
                                             .ToListAsync(cancellationToken);

        var existingFees = await fees.Where(msf => msf.PeriodId == command.PeriodId)
                                     .ToListAsync(cancellationToken);

        foreach (var activeMember in activeMembers.GroupBy(mbr => mbr.MemberId))
        {
            var memberId = activeMember.Key;

            var lastMembership = activeMember.OrderByDescending(mbr => mbr.From)
                                             .First();

            var existing = existingFees.Where(fee => fee.MemberId == memberId)
                                       .ToList();

            switch (existing.Count)
            {
                case 0:
                    fees.Insert(new MembershipFee
                                {
                                    Id = Guid.NewGuid(),
                                    PeriodId = period.Id,
                                    MemberId = memberId,
                                    MembershipTypeId = lastMembership.MembershipTypeId,
                                    Amount = lastMembership.Fee,
                                    State = MembershipFeeState.Due
                                });

                    break;
                case 1:
                    // no update yet
                    break;
                case > 1:
                    // ToDo: Log
                    break;
            }
        }
    }
}