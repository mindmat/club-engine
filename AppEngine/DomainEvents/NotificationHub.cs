using Microsoft.AspNetCore.SignalR;

namespace AppEngine.DomainEvents;


public interface INotificationConsumer
{
    Task Process(Guid? partitionId, string queryName, Guid? rowId);
}

public class NotificationHub : Hub<INotificationConsumer>
{
    public Task SubscribeToPartition(Guid? partitionId)
    {
        if (partitionId != null)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, partitionId.Value.ToString());
        }

        return Task.CompletedTask;
    }

    public Task UnsubscribeFromPartition(Guid? partitionId)
    {
        if (partitionId != null)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, partitionId.Value.ToString());
        }

        return Task.CompletedTask;
    }
}