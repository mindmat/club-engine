using AppEngine.Accounting.Bookings;
using AppEngine.Authorization;
using AppEngine.DataAccess;
using AppEngine.DomainEvents;
using AppEngine.ReadModels;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Accounting.Assignments;

public class CheckIfBookingIsSettledCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public Guid BookingId { get; set; }
}

public class CheckIfBookingIsSettledCommandHandler(IRepository<Booking> bookings,
                                                   ChangeTrigger changeTrigger)
    : IRequestHandler<CheckIfBookingIsSettledCommand>
{
    public async Task Handle(CheckIfBookingIsSettledCommand command, CancellationToken cancellationToken)
    {
        var booking = await bookings.AsTracking()
                                    .Where(pmt => pmt.Id == command.BookingId)
                                    .Include(pmt => pmt.Incoming!)
                                    .ThenInclude(pmt => pmt.Assignments)
                                    .Include(pmt => pmt.Outgoing!)
                                    .ThenInclude(pmt => pmt.Assignments)
                                    .FirstAsync(cancellationToken);

        var incomingSum = booking.Incoming?.Assignments!.Sum(asn => asn.PayoutRequestId == null
                                                                 ? asn.Amount
                                                                 : -asn.Amount)
                       ?? 0;

        var outgoingSum = booking.Outgoing?.Assignments!.Sum(asn => asn.PayoutRequestId == null
                                                                 ? asn.Amount
                                                                 : -asn.Amount)
                       ?? 0;

        var balance = booking.Amount
                    - incomingSum
                    - outgoingSum;
        //+ incomingPayment.RepaymentAssignments!.Sum(asn => asn.Amount);
        var settled = balance == 0m;

        if (settled != booking.Settled_ReadModel)
        {
            booking.Settled_ReadModel = settled;
            var partitionId = booking.PartitionId;

            if (partitionId != null)
            {
                changeTrigger.QueryChanged<PaymentsByDayQuery>(partitionId.Value);
            }
        }
    }
}