using AppEngine.TimeHandling;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.DomainEvents;

public class SaveDomainEventCommand : IRequest
{
    public Guid DomainEventId { get; set; }
    public Guid? DomainEventId_Parent { get; set; }
    public required string EventData { get; set; }
    public Guid? PartitionId { get; set; }
    public required string EventType { get; set; }
}

public class SaveDomainEventCommandHandler(DbContext dbContext,
                                           RequestTimeProvider timeProvider) : IRequestHandler<SaveDomainEventCommand>
{
    private readonly DbSet<PersistedDomainEvent> _domainEvents = dbContext.Set<PersistedDomainEvent>();

    public async Task Handle(SaveDomainEventCommand command, CancellationToken cancellationToken)
    {
        var id = command.DomainEventId == Guid.Empty ? Guid.NewGuid() : command.DomainEventId;

        await _domainEvents.AddAsync(new PersistedDomainEvent
                                     {
                                         Id = id,
                                         PartitionId = command.PartitionId,
                                         DomainEventId_Parent = command.DomainEventId_Parent,
                                         Timestamp = timeProvider.RequestNow,
                                         Type = command.EventType,
                                         Data = command.EventData
                                     },
                                     cancellationToken);
    }
}