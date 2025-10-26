using AppEngine.Accounting.Bookings;
using AppEngine.Authorization;
using AppEngine.Types;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Accounting.Assignments;

public class PaymentAssignmentsQuery : IRequest<PaymentAssignments>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public Guid PaymentId { get; set; }
    public string? SearchString { get; set; }
}

public class PaymentAssignmentsQueryHandler(IQueryable<Booking> bookings,
                                            //IQueryable<PayoutRequest> payoutRequests,
                                            IQueryable<BookingAssignment> assignments,
                                            IEnumerable<IPaymentAssignmentSource> assignmentSources)
    : IRequestHandler<PaymentAssignmentsQuery, PaymentAssignments>
{
    public async Task<PaymentAssignments> Handle(PaymentAssignmentsQuery query,
                                                 CancellationToken cancellationToken)
    {
        var payment = await bookings.Where(pmt => pmt.Id == query.PaymentId
                                               && pmt.PartitionId == query.PartitionId)
                                    .Include(pmt => pmt.Incoming!.Assignments!)
                                    .ThenInclude(pas => pas.OutgoingPayment!.Booking)
                                    .Include(pmt => pmt.Outgoing!.Assignments!)
                                    .ThenInclude(pas => pas.PayoutRequest)
                                    .FirstAsync(cancellationToken);

        var message = payment.Message;

        if (string.IsNullOrWhiteSpace(message))
        {
            message = payment.Info;
        }

        var otherParty = string.Empty;
        var result = new PaymentAssignments { Ignored = payment.Ignore };
        var userSearch = !string.IsNullOrWhiteSpace(query.SearchString);

        if (payment.Incoming != null)
        {
            // Incoming payment -> find fees
            otherParty = payment.Incoming.DebitorName;
            result.Type = PaymentType.Incoming;

            result.OpenAmount = payment.Amount
                              - payment.Incoming.Assignments!.Sum(asn => asn.PayoutRequestId == null && asn.OutgoingPaymentId == null
                                                                      ? asn.Amount
                                                                      : -asn.Amount);

            result.ExistingAssignments = payment.Incoming
                                                .Assignments!
                                                .Where(pas => pas is
                                                {
                                                    SourceId: not null,
                                                    SourceType: not null,
                                                    PaymentAssignmentId_Counter: null
                                                })
                                                .Select(pas => new ExistingAssignment
                                                               {
                                                                   PaymentAssignmentId_Existing = pas.Id,
                                                                   BookingId = payment.Id,
                                                                   SourceId = pas.SourceId!.Value,
                                                                   SourceType = pas.SourceType!,
                                                                   AssignedAmount = pas.Amount
                                                               })
                                                .ToList();

            //result.AssignedRepayments = payment.Incoming
            //                                   .Assignments!
            //                                   .Where(pas => pas.SourceId == null
            //                                              && pas.OutgoingPaymentId != null)
            //                                   .Select(pas => new AssignedRepayment
            //                                                  {
            //                                                      PaymentAssignmentId = pas.Id,
            //                                                      CreditorName = pas.OutgoingPayment!.CreditorName,
            //                                                      CreditorIban = pas.OutgoingPayment!.CreditorIban,
            //                                                      AssignedAmount = pas.Amount,
            //                                                      PaymentDate = pas.OutgoingPayment.Booking!.BookingDate
            //                                                  })
            //                                   .ToList();

            //// find repayment candidates
            //var payments = await bookings.Where(pmt => pmt.PartitionId == query.PartitionId
            //                                         && !pmt.Settled_ReadModel
            //                                         && pmt.Type == PaymentType.Outgoing)
            //                              .WhereIf(search, pmt => EF.Functions.Like(pmt.Outgoing!.CreditorName!, $"%{query.SearchString}%"))
            //                              .Select(pmt => new RepaymentCandidate
            //                                             {
            //                                                 PaymentId_Incoming = payment.Id,
            //                                                 PaymentId_Outgoing = pmt.Id,
            //                                                 BookingDate = pmt.BookingDate,
            //                                                 Amount = pmt.Amount,
            //                                                 AmountUnsettled = pmt.Amount
            //                                                                 - pmt.Outgoing!.Assignments!
            //                                                                      .Select(asn => asn.PayoutRequestId == null
            //                                                                                  ? asn.Amount
            //                                                                                  : -asn.Amount)
            //                                                                      .Sum(),
            //                                                 Settled = pmt.Settled_ReadModel,
            //                                                 Currency = pmt.Currency,
            //                                                 Info = pmt.Info,
            //                                                 CreditorName = pmt.Outgoing.CreditorName
            //                                             })
            //                              .ToListAsync(cancellationToken);

            //if (!search)
            //{
            //    payments.ForEach(pmt => pmt.MatchScore = CalculateMatchScore(pmt, payment.Incoming));

            //    result.RepaymentCandidates = payments.Where(pmt => pmt.MatchScore > 1)
            //                                         .OrderByDescending(mtc => mtc.MatchScore);
            //}
            //else
            //{
            //    result.RepaymentCandidates = payments.OrderByDescending(mtc => mtc.MatchScore);
            //}
        }

        if (payment.Outgoing != null)
        {
            // Outgoing payment -> find repayments & payout requests
            otherParty = payment.Outgoing.CreditorName;
            result.Type = PaymentType.Outgoing;

            result.OpenAmount = payment.Amount
                              - payment.Outgoing.Assignments!.Sum(asn => asn.Amount);

            result.ExistingAssignments = payment.Outgoing.Assignments!
                                                .Where(pas => pas.SourceId != null
                                                           && pas.SourceType != null
                                                           && pas.PaymentAssignmentId_Counter == null)
                                                .Select(pas => new ExistingAssignment
                                                               {
                                                                   PaymentAssignmentId_Existing = pas.Id,
                                                                   BookingId = payment.Id,
                                                                   SourceId = pas.SourceId!.Value,
                                                                   SourceType = pas.SourceType!,
                                                                   AssignedAmount = pas.Amount
                                                               })
                                                .ToList();

            //var payoutRequestCandidates = await payoutRequests.Where(prq => prq.PartitionId == query.PartitionId
            //                                                             && prq.State != PayoutState.Confirmed)
            //                                                  .WhereIf(search,
            //                                                           pmt => EF.Functions.Like(pmt.Registration!.RespondentFirstName!, $"%{query.SearchString}%")
            //                                                               || EF.Functions.Like(pmt.Registration!.RespondentLastName!, $"%{query.SearchString}%"))
            //                                                  .Select(prq => new PayoutRequestCandidate
            //                                                                 {
            //                                                                     PayoutRequestId = prq.Id,
            //                                                                     Participant = $"{prq.Registration!.RespondentFirstName} {prq.Registration.RespondentLastName}",
            //                                                                     Amount = prq.Amount,
            //                                                                     AmountUnsettled = prq.Amount - prq.Assignments!.Sum(pas => pas.Amount),
            //                                                                     Info = prq.Reason,
            //                                                                     IbanProposed = prq.IbanProposed
            //                                                                 })
            //                                                  .Where(prq => prq.AmountUnsettled > 0)
            //                                                  .ToListAsync(cancellationToken);

            //if (!search)
            //{
            //    payoutRequestCandidates.ForEach(pmt => pmt.MatchScore = CalculateMatchScore(pmt, payment.Outgoing));

            //    result.PayoutRequestCandidates = payoutRequestCandidates.Where(mat => mat.MatchScore > 1)
            //                                                            .OrderByDescending(mtc => mtc.MatchScore);
            //}
            //else
            //{
            //    result.PayoutRequestCandidates = payoutRequestCandidates.OrderByDescending(mtc => mtc.MatchScore);
            //}

            //result.AssignedPayoutRequests = payment.Outgoing.Assignments!
            //                                       .Where(pas => pas.Registration != null
            //                                                  && pas.PayoutRequestId != null)
            //                                       .Select(pas => new AssignedPayoutRequest
            //                                                      {
            //                                                          PaymentAssignmentId = pas.Id,
            //                                                          PayoutRequestId = pas.PayoutRequestId!.Value,
            //                                                          Participant = $"{pas.Registration!.RespondentFirstName} {pas.Registration.RespondentLastName}",
            //                                                          Amount = pas.Amount,
            //                                                          Info = pas.PayoutRequest?.Reason
            //                                                      })
            //                                       .ToList();
        }

        if (result.OpenAmount != 0
         && payment.PartitionId != null
         && (!string.IsNullOrWhiteSpace(message)
          || !string.IsNullOrWhiteSpace(otherParty)))
        {
            var candidates = (await assignmentSources.SelectManyAsync(ass => GetCandidates(ass,
                                                                                           payment.Id,
                                                                                           payment.PartitionId.Value,
                                                                                           query.SearchString,
                                                                                           message,
                                                                                           otherParty,
                                                                                           cancellationToken)))
                .ToList();

            await AddExistingAssignmentInfos(candidates,
                                             payment.Amount,
                                             payment.PartitionId.Value);
            CalculateMatchScores(payment, otherParty, candidates);

            result.SourceCandidates = candidates.WhereIf(userSearch, mat => mat.MatchScore > 0)
                                                .OrderByDescending(mtc => mtc.MatchScore);
        }

        foreach (var assignmentSource in assignmentSources)
        {
            var assignmentsOfThisType = result.ExistingAssignments?
                                              .Where(ass => ass.SourceType == assignmentSource.Type)
                                              .ToList()
                                     ?? [];

            if (assignmentsOfThisType.Count > 0)
            {
                var sourceInfos = await assignmentSource.GetSourceInfos(query.PartitionId,
                                                                        assignmentsOfThisType.Select(ass => ass.SourceId));

                foreach (var assignment in assignmentsOfThisType)
                {
                    if (sourceInfos.TryGetValue(assignment.SourceId, out var sourceInfo))
                    {
                        assignment.TextPrimary = sourceInfo.TextPrimary;
                        assignment.TextSecondary = sourceInfo.TextSecondary;
                        assignment.Tags = sourceInfo.Tags;
                    }
                }
            }
        }

        return result;
    }

    private void CalculateMatchScores(Booking payment, string? otherParty, List<AssignmentCandidate> candidates)
    {
        var wordsInPayment = payment.Message?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(wrd => wrd.ToLowerInvariant())
                                    .ToHashSet();

        foreach (var candidate in candidates)
        {
            candidate.MatchScore = CalculateMatchScore(candidate, wordsInPayment, otherParty);
        }
    }

    private async Task AddExistingAssignmentInfos(List<AssignmentCandidate> candidates, decimal paymentAmount, Guid partitionId)
    {
        var sourceIds = candidates.Select(sca => sca.SourceId)
                                  .Distinct();

        var sourceAssignments = await assignments.Where(bas => (bas.IncomingPayment!.Booking!.PartitionId == partitionId
                                                             || bas.OutgoingPayment!.Booking!.PartitionId == partitionId)
                                                            && sourceIds.Contains(bas.SourceId!.Value))
                                                 .GroupBy(bas => new { bas.SourceType, bas.SourceId })
                                                 .ToDictionaryAsync(grp => grp.Key,
                                                                    grp => grp.Sum(bas => bas.OutgoingPaymentId == null
                                                                                       ? bas.Amount
                                                                                       : -bas.Amount));

        foreach (var candidate in candidates)
        {
            var amountAssigned = sourceAssignments.Where(bas => bas.Key.SourceType == candidate.SourceType
                                                             && bas.Key.SourceId == candidate.SourceId)
                                                  .Sum(bas => bas.Value);

            candidate.AmountOpen = candidate.AmountTotal - amountAssigned;
            candidate.AmountMatch = paymentAmount == candidate.AmountTotal;
        }
    }

    private async Task<IEnumerable<AssignmentCandidate>> GetCandidates(IPaymentAssignmentSource source,
                                                                       Guid bookingId,
                                                                       Guid partitionId,
                                                                       string? searchString,
                                                                       string? message,
                                                                       string? otherParty,
                                                                       CancellationToken cancellationToken)
    {
        var candidates = (await source.GetCandidates(partitionId,
                                                     searchString,
                                                     message,
                                                     otherParty,
                                                     cancellationToken))
            .ToList();

        if (candidates.Count == 0)
        {
            return [];
        }

        return candidates.Select(sca => new AssignmentCandidate
                                        {
                                            BookingId = bookingId,
                                            SourceType = source.Type,
                                            SourceId = sca.SourceId,
                                            TextPrimary = sca.TextPrimary,
                                            TextSecondary = sca.TextSecondary,

                                            AmountTotal = sca.AmountTotal
                                        });
    }

    private static int CalculateMatchScore(AssignmentCandidate candidate,
                                           IReadOnlySet<string>? wordsInPayment,
                                           string? otherParty)
    {
        var score = 0;

        var nameWords = Enumerable.Union(candidate.TextPrimary?.Split(' ') ?? [],
                                         candidate.TextSecondary?.Split(' ') ?? [])
                                  .Select(nmw => nmw.ToLowerInvariant())
                                  .ToList();

        if (wordsInPayment != null)
        {
            // names can contain multiple words, e.g. 'de Luca'
            score = nameWords.Sum(nmw => wordsInPayment.Count(wrd => wrd == nmw));
        }

        if (otherParty != null)
        {
            var wordsInDebitor = otherParty.Split(' ')
                                           .Select(nmw => nmw.ToLowerInvariant())
                                           .ToList();

            score += nameWords.Sum(nmw => wordsInDebitor.Count(wrd => wrd == nmw));
        }

        return score;
    }

    private static int CalculateMatchScore(RepaymentCandidate paymentCandidate, IncomingPayment openPayment)
    {
        var creditorParts = paymentCandidate.CreditorName?.Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries)
                                            ?.Select(wrd => wrd.ToLowerInvariant())
                         ?? [];

        var wordsInCandidate = paymentCandidate.Info?.Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries)
                                               ?.Select(wrd => wrd.ToLowerInvariant())
                                               ?.ToList()
                            ?? [];

        var unsettledAmountInOpenPayment = openPayment.Booking!.Amount
                                         - openPayment.Assignments!.Sum(asn => asn.PayoutRequestId == null
                                                                            ? asn.Amount
                                                                            : -asn.Amount);

        var wordsInOpenPayment = openPayment.Booking!.Message?
                                            .Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries)
                                            .Select(wrd => wrd.ToLowerInvariant())
                                            .ToHashSet()
                              ?? [];

        return wordsInOpenPayment.Sum(opw => wordsInCandidate.Count(cdw => cdw == opw))
             + (wordsInOpenPayment.Sum(opw => creditorParts.Count(cdw => cdw == opw)) * 10)
             + (paymentCandidate.AmountUnsettled == unsettledAmountInOpenPayment ? 5 : 0);
    }

    private static int CalculateMatchScore(PayoutRequestCandidate payoutRequestCandidate, OutgoingPayment openPayment)
    {
        var creditorParts = payoutRequestCandidate.Participant?.Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries)
                                                  ?.Select(wrd => wrd.ToLowerInvariant())
                         ?? new List<string>();

        var unsettledAmountInOpenPayment = openPayment.Booking!.Amount
                                         - openPayment.Assignments!.Sum(asn => asn.PayoutRequestId == null
                                                                            ? asn.Amount
                                                                            : -asn.Amount);

        var wordsInOpenPayment = openPayment.Booking!.Message?
                                            .Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries)
                                            .Select(wrd => wrd.ToLowerInvariant())
                                            .ToHashSet()
                              ?? new HashSet<string>(0);
        ;

        return (wordsInOpenPayment.Sum(opw => creditorParts.Count(cdw => cdw == opw)) * 10)
             + (payoutRequestCandidate.AmountUnsettled == unsettledAmountInOpenPayment ? 5 : 0)
             + (payoutRequestCandidate.IbanProposed == openPayment.CreditorIban ? 50 : 0);
    }
}

public class PaymentAssignments
{
    public decimal OpenAmount { get; set; }
    public PaymentType Type { get; set; }
    public bool Ignored { get; set; }

    public IEnumerable<AssignmentCandidate>? SourceCandidates { get; set; }
    public IEnumerable<ExistingAssignment>? ExistingAssignments { get; set; }

    //public IEnumerable<RepaymentCandidate>? RepaymentCandidates { get; set; }
    //public IEnumerable<AssignedRepayment>? AssignedRepayments { get; set; }

    //public IEnumerable<PayoutRequestCandidate>? PayoutRequestCandidates { get; set; }
    //public IEnumerable<AssignedPayoutRequest>? AssignedPayoutRequests { get; set; }
}

public class AssignmentCandidate
{
    public Guid SourceId { get; set; }
    public string SourceType { get; set; }
    public string? TextPrimary { get; set; }
    public string? TextSecondary { get; set; }
    public string[] Tags { get; set; }
    public decimal AmountTotal { get; set; }
    public decimal AmountOpen { get; set; }
    public bool AmountMatch { get; set; }
    public int MatchScore { get; set; }
    public Guid BookingId { get; set; }
}

public class ExistingAssignment
{
    public Guid SourceId { get; set; }
    public string SourceType { get; set; }
    public string? TextPrimary { get; set; }
    public string? TextSecondary { get; set; }
    public string[]? Tags { get; set; }

    public Guid PaymentAssignmentId_Existing { get; set; }
    public decimal? AssignedAmount { get; set; }


    public Guid BookingId { get; set; }
}

public class RepaymentCandidate
{
    public decimal Amount { get; set; }
    public decimal AmountUnsettled { get; set; }
    public DateTime BookingDate { get; set; }
    public string? Currency { get; set; }
    public string? CreditorName { get; set; }
    public string? Info { get; set; }
    public int MatchScore { get; set; }
    public Guid PaymentId_Outgoing { get; set; }
    public Guid PaymentId_Incoming { get; set; }
    public bool Settled { get; set; }
}

public class AssignedRepayment
{
    public Guid PaymentAssignmentId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? CreditorName { get; set; }
    public string? CreditorIban { get; set; }
    public decimal? AssignedAmount { get; set; }
}

public class PayoutRequestCandidate
{
    public Guid PayoutRequestId { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountUnsettled { get; set; }
    public string? Participant { get; set; }
    public string? Info { get; set; }
    public int MatchScore { get; set; }
    public string? IbanProposed { get; set; }
}

public class AssignedPayoutRequest
{
    public Guid PaymentAssignmentId { get; set; }
    public Guid PayoutRequestId { get; set; }
    public decimal Amount { get; set; }
    public string? Participant { get; set; }
    public string? Info { get; set; }
}