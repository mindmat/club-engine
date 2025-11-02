using AppEngine.Accounting.Bookings;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Accounting;

public class AccountingFacade(IQueryable<BookingAssignment> assignments)
{
    public async Task<Dictionary<PaymentAssignee, decimal>> GetAssignedAmount(Guid partitionId, IEnumerable<PaymentAssignee> sourceIds)
    {
        return await assignments.Where(bas => (bas.IncomingPayment!.Booking!.PartitionId == partitionId
                                            || bas.OutgoingPayment!.Booking!.PartitionId == partitionId)
                                           && sourceIds.Select(pas => pas.SourceId).Contains(bas.SourceId!.Value)
                                           && bas.SourceType != null)
                                .GroupBy(bas => new PaymentAssignee(bas.SourceType!, bas.SourceId!.Value))
                                .ToDictionaryAsync(grp => grp.Key,
                                                   grp => grp.Sum(bas => bas.OutgoingPaymentId == null
                                                                      ? bas.Amount
                                                                      : -bas.Amount));
    }
}

public record PaymentAssignee(string SourceType, Guid SourceId);