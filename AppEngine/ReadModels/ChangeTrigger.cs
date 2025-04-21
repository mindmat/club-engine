using AppEngine.Authorization;
using AppEngine.DomainEvents;
using AppEngine.Partitions;
using AppEngine.ServiceBus;
using AppEngine.TimeHandling;

using MediatR;

namespace AppEngine.ReadModels;

public class ChangeTrigger(CommandQueue commandQueue,
                           RequestTimeProvider timeProvider,
                           PartitionContext partitionContext,
                           IEnumerable<IReadModelCalculator> calculators,
                           IEventBus eventBus)
{
    public void TriggerUpdate<T>(Guid? rowId = null,
                                 Guid? eventId = null,
                                 bool publishEvenWhenDbCommitFails = false,
                                 TimeSpan? delay = null)
        where T : IReadModelCalculator
    {
        eventId ??= partitionContext.PartitionId
                 ?? throw new ArgumentNullException(nameof(eventId));

        var queryName = calculators.First(cal => cal.GetType() == typeof(T))
                                   .QueryName;

        commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                    {
                                        QueryName = queryName,
                                        PartitionId = eventId.Value,
                                        RowId = rowId,
                                        DirtyMoment = timeProvider.RequestNow
                                    },
                                    publishEvenWhenDbCommitFails,
                                    delay);
    }

    public void QueryChanged<TQuery>(Guid partitionId, Guid? rowId = null, bool publishEvenWhenDbCommitFails = false)
        where TQuery : IPartitionBoundRequest
    {
        eventBus.Publish(new QueryChanged
                         {
                             PartitionId = partitionId,
                             QueryName = typeof(TQuery).Name,
                             RowId = rowId
                         },
                         publishEvenWhenDbCommitFails);
    }

    public void QueryChanged(string queryName,
                             Guid partitionId,
                             Guid? rowId = null,
                             bool publishEvenWhenDbCommitFails = false)
    {
        eventBus.Publish(new QueryChanged
                         {
                             PartitionId = partitionId,
                             QueryName = queryName,
                             RowId = rowId
                         },
                         publishEvenWhenDbCommitFails);
    }


    public void PublishEvent<TEvent>(TEvent @event)
        where TEvent : DomainEvent
    {
        eventBus.Publish(@event);
    }

    public void EnqueueCommand<T>(T command)
        where T : IRequest
    {
        commandQueue.EnqueueCommand(command);
    }
}