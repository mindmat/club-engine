using AppEngine.Accounting.Assignments;
using AppEngine.DomainEvents;

using ClubEngine.ApiService.MembershipFees;

using MediatR;

namespace ClubEngine.ApiService.Accounting
{
    public class RecalculateWhenFeeAssignmentChanged(IQueryable<MembershipFee> fees) : IEventToCommandTranslation<SourceAssignmentsChanged>
    {
        public IEnumerable<IRequest> Translate(SourceAssignmentsChanged e)
        {
            if (e is { SourceType: FeesSource.SourceType, SourceId: not null })
            {
                var member = fees.Where(fee => fee.Id == e.SourceId)
                                 .Select(fee => new
                                                {
                                                    fee.MemberId,
                                                    fee.Member!.ClubId
                                                })
                                 .FirstOrDefault();

                if (member != null)
                {
                    yield return new UpdateFeeAmountPaidCommand
                                 {
                                     PartitionId = member.ClubId,
                                     MembershipFeeId = e.SourceId.Value
                                 };
                }
            }
        }
    }
}