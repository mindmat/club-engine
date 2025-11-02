using AppEngine.Accounting;
using AppEngine.Authorization;
using AppEngine.DataAccess;
using AppEngine.ReadModels;
using AppEngine.Types;

using ClubEngine.ApiService.Members;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.MembershipFees;

public class UpdateFeeAmountPaidCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public Guid MembershipFeeId { get; set; }
}

public class UpdateFeeAmountPaidCommandHandler(IRepository<MembershipFee> fees,
                                               AccountingFacade accounting,
                                               ChangeTrigger changeTrigger) : IRequestHandler<UpdateFeeAmountPaidCommand>
{
    public async Task Handle(UpdateFeeAmountPaidCommand command, CancellationToken cancellationToken)
    {
        var fee = await fees.AsTracking()
                            .FirstAsync(fee => fee.Id == command.MembershipFeeId
                                            && fee.Member!.ClubId == command.PartitionId,
                                        cancellationToken);

        var assignedAmounts = await accounting.GetAssignedAmount(command.PartitionId, [new PaymentAssignee(FeesSource.SourceType, fee.Id)]);
        fee.AmountPaid_ReadModel = assignedAmounts.Lookup(new PaymentAssignee(FeesSource.SourceType, fee.Id));

        if (fee.State != MembershipFeeState.Cancelled)
        {
            fee.State = fee.AmountPaid_ReadModel >= fee.Amount
                ? MembershipFeeState.Paid
                : MembershipFeeState.Due;
        }

        changeTrigger.TriggerUpdate<MemberCalculator>(command.PartitionId, fee.MemberId);
    }
}