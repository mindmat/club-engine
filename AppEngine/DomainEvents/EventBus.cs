using AppEngine.Authorization.UsersInPartition;
using AppEngine.Json;
using AppEngine.Partitions;
using AppEngine.ReadModels;
using AppEngine.ServiceBus;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace AppEngine.DomainEvents;

public class EventBus(IServiceProvider services,
                      CommandQueue commandQueue,
                      PartitionContext partitionContext,
                      AuthenticatedUserId user,
                      Serializer serializer,
                      IHubContext<NotificationHub, INotificationConsumer> hub)
    : IEventBus
{
    private readonly List<(QueryChanged Event, bool PublishAnyway)> _notifications = [];

    public void Publish<TEvent>(TEvent @event, bool publishEvenWhenDbCommitFails = false)
        where TEvent : DomainEvent
    {
        // try to fill out missing data
        if (@event.Id == Guid.Empty)
        {
            @event.Id = Guid.NewGuid();
        }

        @event.UserId ??= user.UserId;
        @event.PartitionId ??= partitionContext.PartitionId;

        //var translations = services.GetServices<IEventToCommandTranslation<TEvent>>().ToList();
        //foreach (var command in translations.SelectMany(trn => trn.Translate(@event)))
        //{
        //    commandQueue.EnqueueCommand(command);
        //}
        var translations = services.GetServices<IEventToQueryChangedTranslation<TEvent>>().ToList();

        foreach (var queryChanged in translations.SelectMany(trn => trn.Translate(@event)))
        {
            _notifications.Add((queryChanged, publishEvenWhenDbCommitFails));
        }

        if (@event is QueryChanged queryChangedEvent)
        {
            _notifications.Add((queryChangedEvent, publishEvenWhenDbCommitFails));
        }
        else
        {
            commandQueue.EnqueueCommand(new SaveDomainEventCommand
                                        {
                                            DomainEventId = @event.Id,
                                            DomainEventId_Parent = @event.DomainEventId_Parent,
                                            PartitionId = @event.PartitionId ?? partitionContext.PartitionId,
                                            EventType = @event.GetType().FullName!,
                                            EventData = serializer.Serialize(@event)
                                        });
        }
    }

    public void Release(bool dbCommitSucceeded)
    {
        foreach (var notification in _notifications.Where(ntf => dbCommitSucceeded || ntf.PublishAnyway))
        {
            if (notification.Event.PartitionId != null)
            {
                hub.Clients.Group(notification.Event.PartitionId.ToString()!)
                   .Process(notification.Event.PartitionId, notification.Event.QueryName, notification.Event.RowId);
            }
            else
            {
                hub.Clients.All.Process(null, notification.Event.QueryName, notification.Event.RowId);
            }
        }
    }
}