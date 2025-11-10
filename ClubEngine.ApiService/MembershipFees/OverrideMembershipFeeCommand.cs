using AppEngine.Authorization;
using AppEngine.ReadModels;

using ClubEngine.ApiService.Members;

namespace ClubEngine.ApiService.MembershipFees;

public class OverrideMembershipFeeCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public Guid MemberId { get; set; }
    public decimal? Amount { get; set; }
}

public class OverrideMembershipFeeCommandHandler(IQueryable<Member> members,
                                                 ChangeTrigger changeTrigger)
    : IRequestHandler<OverrideMembershipFeeCommand>
{
    public async Task Handle(OverrideMembershipFeeCommand command, CancellationToken cancellationToken)
    {
        var member = await members.AsTracking()
                                  .Where(mbr => mbr.ClubId == command.PartitionId
                                             && mbr.Id == command.MemberId)
                                  .FirstAsync(cancellationToken);

        if (member.FeeOverride != command.Amount)
        {
            member.FeeOverride = command.Amount;
            changeTrigger.TriggerUpdate<MemberCalculator>(member.ClubId, member.Id);
            changeTrigger.QueryChanged<MembershipFeesQuery>(member.ClubId);
        }
    }
}