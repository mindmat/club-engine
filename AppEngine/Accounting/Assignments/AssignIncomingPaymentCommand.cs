using AppEngine.Accounting.Bookings;
using AppEngine.Authorization;
using AppEngine.Authorization.UsersInPartition;
using AppEngine.DataAccess;
using AppEngine.DomainEvents;
using AppEngine.Internationalization;
using AppEngine.ReadModels;
using AppEngine.TimeHandling;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Accounting.Assignments;

public class AssignIncomingPaymentCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public Guid PaymentIncomingId { get; set; }
    public Guid SourceId { get; set; }
    public string SourceType { get; set; }
    public decimal Amount { get; set; }
    public bool AcceptDifference { get; set; }
    public string? AcceptDifferenceReason { get; set; }
}

public class AssignIncomingPaymentCommandHandler(IQueryable<IncomingPayment> incomingPayments,
                                                 IRepository<BookingAssignment> assignments,
                                                 IEnumerable<IPaymentAssignmentSource> assignmentSources,
                                                 //IRepository<IndividualReduction> individualReductions,
                                                 IEventBus eventBus,
                                                 AuthenticatedUserId userId,
                                                 RequestTimeProvider dateTimeProvider,
                                                 ChangeTrigger changeTrigger)
    : IRequestHandler<AssignIncomingPaymentCommand>
{
    public async Task Handle(AssignIncomingPaymentCommand command, CancellationToken cancellationToken)
    {
        var source = assignmentSources.Single(ass => ass.Type == command.SourceType);
        var sourceCandidate = (await source.GetSourceInfos(command.PartitionId, [command.SourceId]))[command.SourceId];
        var incomingPayment = await incomingPayments.FirstAsync(pmt => pmt.Id == command.PaymentIncomingId, cancellationToken);

        var assignment = new BookingAssignment
                         {
                             Id = Guid.NewGuid(),
                             SourceType = source.Type,
                             SourceId = command.SourceId,
                             IncomingPaymentId = incomingPayment.Id,
                             Amount = command.Amount,
                             Created = dateTimeProvider.RequestNow
                         };
        assignments.Insert(assignment);

        //if (command.AcceptDifference)
        //{
        //    var difference = registration.Price_AdmittedAndReduced
        //                   - registration.PaymentAssignments!.Sum(pmt => pmt.OutgoingPayment == null
        //                                                                     ? pmt.Amount
        //                                                                     : -pmt.Amount);
        //    await individualReductions.InsertOrUpdateEntity(new IndividualReduction
        //                                                    {
        //                                                        Id = Guid.NewGuid(),
        //                                                        RegistrationId = registration.Id,
        //                                                        Amount = difference,
        //                                                        Reason = command.AcceptDifferenceReason,
        //                                                        UserId = userId.UserId ?? Guid.Empty
        //                                                    }, cancellationToken);

        //    eventBus.Publish(new IndividualReductionAdded
        //                     {
        //                         RegistrationId = registration.Id,
        //                         Amount = difference,
        //                         Reason = command.AcceptDifferenceReason
        //                     });
        //}

        eventBus.Publish(new IncomingPaymentAssigned
                         {
                             PaymentAssignmentId = assignment.Id,
                             Amount = assignment.Amount,
                             SourceType = source.Type,
                             SourceId = command.SourceId,
                             IncomingPaymentId = incomingPayment.Id,
                             SourceText = sourceCandidate.TextPrimary
                         });


        //changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
        //changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, registration.EventId);
        changeTrigger.QueryChanged<PaymentsByDayQuery>(command.PartitionId);
        changeTrigger.QueryChanged<PaymentAssignmentsQuery>(command.PartitionId, incomingPayment.Id);

        changeTrigger.PublishEvent(new SourceAssignmentsChanged
                                   {
                                       SourceType = source.Type,
                                       SourceId = command.SourceId,
                                   });
    }
}

public class IncomingPaymentAssigned : DomainEvent
{
    public Guid IncomingPaymentId { get; set; }
    public Guid PaymentAssignmentId { get; set; }
    public decimal Amount { get; set; }
    public string SourceType { get; set; }
    public Guid SourceId { get; set; }

    public string? Debitor { get; set; }
    public string? SourceText { get; set; }
}

public class IncomingPaymentAssignedUserTranslation(Translator translator) : IEventToUserTranslation<IncomingPaymentAssigned>
{
    public string GetText(IncomingPaymentAssigned domainEvent)
    {
        return $"Zahlungseingang über {domainEvent.Amount} von {domainEvent.Debitor} zu {translator.GetResourceString(domainEvent.SourceType)} {domainEvent.SourceText} zugeordnet.";
    }
}