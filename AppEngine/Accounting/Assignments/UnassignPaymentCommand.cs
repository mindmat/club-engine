using AppEngine.Accounting.Bookings;
using AppEngine.Authorization;
using AppEngine.DataAccess;
using AppEngine.DomainEvents;
using AppEngine.Internationalization;
using AppEngine.ReadModels;
using AppEngine.TimeHandling;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Accounting.Assignments;

public class UnassignPaymentCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public Guid PaymentAssignmentId { get; set; }
}

public class UnassignPaymentCommandHandler(IRepository<BookingAssignment> assignments,
                                           RequestTimeProvider dateTimeProvider,
                                           ChangeTrigger changeTrigger)
    : IRequestHandler<UnassignPaymentCommand>
{
    public async Task Handle(UnassignPaymentCommand command, CancellationToken cancellationToken)
    {
        var existingAssignment = await assignments.FirstAsync(ass => ass.Id == command.PaymentAssignmentId
                                                                  && (ass.IncomingPayment!.Booking!.PartitionId == command.PartitionId
                                                                   || ass.OutgoingPayment!.Booking!.PartitionId == command.PartitionId),
                                                              cancellationToken);

        if (existingAssignment.PaymentAssignmentId_Counter != null)
        {
            throw new ArgumentException($"Assignment {existingAssignment.Id} already has a counter assignment: {existingAssignment.PaymentAssignmentId_Counter}");
        }

        var counterAssignment = new BookingAssignment
                                {
                                    Id = Guid.NewGuid(),
                                    SourceType = existingAssignment.SourceType,
                                    SourceId = existingAssignment.SourceId,
                                    IncomingPaymentId = existingAssignment.IncomingPaymentId,
                                    OutgoingPaymentId = existingAssignment.OutgoingPaymentId,
                                    PaymentAssignmentId_Counter = existingAssignment.Id,
                                    Amount = -existingAssignment.Amount,
                                    Created = dateTimeProvider.RequestNow
                                };
        existingAssignment.PaymentAssignmentId_Counter = counterAssignment.Id;
        assignments.Insert(counterAssignment);

        if (existingAssignment.IncomingPaymentId != null)
        {
            changeTrigger.PublishEvent(new IncomingPaymentUnassigned
                                       {
                                           PartitionId = command.PartitionId,
                                           PaymentAssignmentId = command.PaymentAssignmentId,
                                           PaymentAssignmentId_Counter = counterAssignment.Id,
                                           IncomingPaymentId = existingAssignment.IncomingPaymentId!.Value,
                                           SourceType = existingAssignment.SourceType,
                                           SourceId = existingAssignment.SourceId,
                                           Amount = existingAssignment.Amount
                                       });

            changeTrigger.PublishEvent(new SourceAssignmentsChanged
                                       {
                                           SourceType = existingAssignment.SourceType,
                                           SourceId = existingAssignment.SourceId,
                                       });

            changeTrigger.QueryChanged<PaymentAssignmentsQuery>(command.PartitionId, existingAssignment.IncomingPaymentId);
        }
        else if (existingAssignment.OutgoingPaymentId != null)
        {
            changeTrigger.PublishEvent(new OutgoingPaymentUnassigned
                                       {
                                           PartitionId = command.PartitionId,
                                           PaymentAssignmentId = command.PaymentAssignmentId,
                                           PaymentAssignmentId_Counter = counterAssignment.Id,
                                           OutgoingPaymentId = existingAssignment.OutgoingPaymentId!.Value,
                                           SourceType = existingAssignment.SourceType,
                                           SourceId = existingAssignment.SourceId,
                                           Amount = existingAssignment.Amount
                                       });

            changeTrigger.PublishEvent(new SourceAssignmentsChanged
                                       {
                                           SourceType = existingAssignment.SourceType,
                                           SourceId = existingAssignment.SourceId,
                                       });

            changeTrigger.QueryChanged<PaymentAssignmentsQuery>(command.PartitionId, existingAssignment.OutgoingPaymentId);
        }

        //changeTrigger.TriggerUpdate<RegistrationCalculator>(existingAssignment.RegistrationId, command.EventId);
        //changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, command.EventId);
        changeTrigger.QueryChanged<PaymentsByDayQuery>(command.PartitionId);
    }
}

public class SourceAssignmentsChanged : DomainEvent
{
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
}

public class IncomingPaymentUnassigned : DomainEvent
{
    public Guid PaymentAssignmentId { get; set; }
    public Guid PaymentAssignmentId_Counter { get; set; }
    public Guid IncomingPaymentId { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceType { get; set; }
    public string? SourceText { get; set; }

    public decimal Amount { get; set; }
}

public class IncomingPaymentUnassignedUserTranslation(Translator translator)
    : IEventToUserTranslation<IncomingPaymentUnassigned>
{
    public string GetText(IncomingPaymentUnassigned domainEvent)
    {
        return $"Zuordnung von Zahlung über {domainEvent.Amount} zu {translator.GetResourceString(domainEvent.SourceType)} {domainEvent.SourceText} rückgängig gemacht";
    }
}

public class OutgoingPaymentUnassigned : DomainEvent
{
    public Guid PaymentAssignmentId { get; set; }
    public Guid PaymentAssignmentId_Counter { get; set; }
    public Guid OutgoingPaymentId { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceType { get; set; }
    public string? SourceText { get; set; }
    public decimal Amount { get; set; }
}

public class OutgoingPaymentUnassignedUserTranslation(Translator translator)
    : IEventToUserTranslation<OutgoingPaymentUnassigned>
{
    public string GetText(OutgoingPaymentUnassigned domainEvent)
    {
        return $"Zuordnung von Auszahlung über {domainEvent.Amount} zu Anmeldung {translator.GetResourceString(domainEvent.SourceType)} rückgängig gemacht";
    }
}