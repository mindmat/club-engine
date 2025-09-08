using AppEngine.Accounting.Iso20022.Camt;
using AppEngine.Authorization;
using AppEngine.DataAccess;
using AppEngine.DomainEvents;
using AppEngine.ReadModels;
using AppEngine.Types;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Accounting.Bookings;

public class PaymentsByDayQuery : IRequest<IEnumerable<BookingsOfDay>>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public bool HideIgnored { get; set; }
    public bool HideSettled { get; set; }
    public bool HideIncoming { get; set; }
    public bool HideOutgoing { get; set; }
    public string? SearchString { get; set; }
    public bool ShowAll { get; set; }
}

public class PaymentsByDayQueryHandler(IQueryable<IncomingPayment> incomingBookings,
                                       IQueryable<OutgoingPayment> outgoingBookings)
    : IRequestHandler<PaymentsByDayQuery, IEnumerable<BookingsOfDay>>
{
    const int ResultDefaultDaysLimit = 20;

    public async Task<IEnumerable<BookingsOfDay>> Handle(PaymentsByDayQuery query,
                                                         CancellationToken cancellationToken)
    {
        var payments = Enumerable.Empty<PaymentDisplayItem>();

        if (!query.HideIncoming)
        {
            payments = payments.Concat(await incomingBookings.Where(bbk => bbk.Booking!.PartitionId == query.PartitionId)
                                                             .WhereIf(query.HideIgnored, bbk => !bbk.Booking!.Ignore)
                                                             .WhereIf(query.HideSettled, bbk => !bbk.Booking!.Settled_ReadModel)
                                                             .WhereIf(!string.IsNullOrWhiteSpace(query.SearchString),
                                                                      bbk => EF.Functions.Like(bbk.Booking!.Message!, $"%{query.SearchString}%")
                                                                          || EF.Functions.Like(bbk.DebitorName!, $"%{query.SearchString}%"))
                                                             .Select(bbk => new PaymentDisplayItem
                                                                            {
                                                                                Id = bbk.Id,
                                                                                Typ = CreditDebit.CRDT,
                                                                                Amount = bbk.Booking!.Amount,
                                                                                Charges = bbk.Charges,
                                                                                Message = bbk.Booking!.Message,
                                                                                DebitorName = bbk.DebitorName,
                                                                                Currency = bbk.Booking!.Currency,
                                                                                Reference = bbk.Booking!.Reference,

                                                                                AmountAssigned = bbk.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                                                                                BookingDate = bbk.Booking!.BookingDate,
                                                                                AmountRepaid = bbk.Booking!.Repaid_ReadModel,
                                                                                Settled = bbk.Booking!.Settled_ReadModel,
                                                                                Ignore = bbk.Booking!.Ignore,
                                                                                Balance = bbk.Booking!.PaymentsFile!.Balance
                                                                            })
                                                             .OrderByDescending(bbk => bbk.BookingDate)
                                                             .ThenByDescending(bbk => bbk.Amount)
                                                             .ToListAsync(cancellationToken));
        }

        if (!query.HideOutgoing)
        {
            payments = payments.Concat(await outgoingBookings.Where(bbk => bbk.Booking!.PartitionId == query.PartitionId)
                                                             .WhereIf(query.HideIgnored, bbk => !bbk.Booking!.Ignore)
                                                             .WhereIf(query.HideSettled, bbk => !bbk.Booking!.Settled_ReadModel)
                                                             .WhereIf(!string.IsNullOrWhiteSpace(query.SearchString),
                                                                      bbk => EF.Functions.Like(bbk.Booking!.Message!, $"%{query.SearchString}%")
                                                                          || EF.Functions.Like(bbk.CreditorName!, $"%{query.SearchString}%"))
                                                             .Select(bbk => new PaymentDisplayItem
                                                                            {
                                                                                Id = bbk.Id,
                                                                                Typ = CreditDebit.DBIT,
                                                                                Amount = bbk.Booking!.Amount,
                                                                                Charges = bbk.Charges,
                                                                                Message = bbk.Booking.Message,
                                                                                CreditorName = bbk.CreditorName,
                                                                                CreditorIban = bbk.CreditorIban,
                                                                                Currency = bbk.Booking.Currency,
                                                                                Reference = bbk.Booking.Reference,

                                                                                AmountAssigned = bbk.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                                                                                BookingDate = bbk.Booking.BookingDate,
                                                                                AmountRepaid = bbk.Booking.Repaid_ReadModel,
                                                                                Settled = bbk.Booking.Settled_ReadModel,
                                                                                Ignore = bbk.Booking.Ignore,
                                                                                Balance = bbk.Booking.PaymentsFile!.Balance
                                                                            })
                                                             .OrderByDescending(bbk => bbk.BookingDate)
                                                             .ThenByDescending(bbk => bbk.Amount)
                                                             .ToListAsync(cancellationToken));
        }

        var days = payments.GroupBy(pmt => new { pmt.BookingDate, pmt.Balance })
                           .Select(day => new BookingsOfDay
                                          {
                                              BookingDate = day.Key.BookingDate,
                                              BalanceAfter = day.Key.Balance,
                                              Bookings = day.Select(pmt => new PaymentDisplayItem
                                                                           {
                                                                               Id = pmt.Id,
                                                                               BookingDate = pmt.BookingDate,
                                                                               Typ = pmt.Typ,
                                                                               Currency = pmt.Currency,
                                                                               Amount = pmt.Amount,
                                                                               Charges = pmt.Charges,
                                                                               Message = pmt.Message,
                                                                               DebitorName = pmt.DebitorName,
                                                                               CreditorName = pmt.CreditorName,
                                                                               CreditorIban = pmt.CreditorIban,
                                                                               Reference = pmt.Reference,

                                                                               AmountAssigned = pmt.AmountAssigned,
                                                                               AmountRepaid = pmt.AmountRepaid,
                                                                               Ignore = pmt.Ignore,
                                                                               Settled = pmt.Settled
                                                                           })
                                          })
                           .OrderByDescending(day => day.BookingDate)
                           .TakeIf(!query.ShowAll, ResultDefaultDaysLimit + 1)
                           .ToList();

        var hasMore = !query.ShowAll
                   && days.Count > ResultDefaultDaysLimit;

        return days;
    }
}

public class BookingsOfDay
{
    public DateTime BookingDate { get; set; }
    public IEnumerable<PaymentDisplayItem> Bookings { get; set; } = null!;
    public decimal? BalanceAfter { get; set; }
}

public class PaymentDisplayItem
{
    public Guid Id { get; set; }
    public CreditDebit? Typ { get; set; }
    public decimal Amount { get; set; }
    public decimal? Charges { get; set; }
    public decimal AmountAssigned { get; set; }
    public DateTime BookingDate { get; set; }
    public string? Currency { get; set; }
    public string? Reference { get; set; }
    public decimal? AmountRepaid { get; set; }
    public bool Settled { get; set; }
    public bool Ignore { get; set; }
    public string? Message { get; set; }
    public string? DebitorName { get; set; }
    public string? CreditorName { get; set; }
    public string? CreditorIban { get; set; }
    public decimal? Balance { get; set; }
}

public class PaymentsByDayQueryChangedWhenPaymentFileProcessed : IEventToQueryChangedTranslation<PaymentFileProcessed>
{
    public IEnumerable<QueryChanged> Translate(PaymentFileProcessed e)
    {
        yield return new QueryChanged
                     {
                         Id = Guid.NewGuid(),
                         QueryName = nameof(PaymentsByDayQuery),
                         PartitionId = e.PartitionId,
                         DomainEventId_Parent = e.Id,
                         UserId = e.UserId
                     };
    }
}