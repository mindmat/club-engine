using AppEngine.Authorization.UsersInPartition;
using AppEngine.ServiceBus;

namespace AppEngine.Authorization;

public interface IAuthorizationChecker
{
    Task ThrowIfUserHasNotRight(Guid eventId, string requestTypeName);
    Task<bool> UserHasRight(Guid eventId, string requestTypeName);
}

internal class AuthorizationChecker(AuthenticatedUserId user,
                                    SourceQueueProvider sourceQueueProvider,
                                    RightsOfUserInPartitionCache cache)
    : IAuthorizationChecker
{
    public async Task ThrowIfUserHasNotRight(Guid eventId, string requestTypeName)
    {
        if (sourceQueueProvider.SourceQueueName != null)
            // message from a queue, no user is authenticated
        {
            return;
        }

        if (!user.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("You are not authenticated");
        }

        var rightsOfUserInEvent = await cache.GetRightsOfUserInPartition(user.UserId.Value, eventId);
        if (!rightsOfUserInEvent.Contains(requestTypeName))
        {
            throw new UnauthorizedAccessException($"You ({user.UserId}) are not authorized for {requestTypeName} in event {eventId}");
        }
    }

    public async Task<bool> UserHasRight(Guid eventId, string requestTypeName)
    {
        if (sourceQueueProvider.SourceQueueName != null)
            // message from a queue, no user is authenticated
        {
            return true;
        }

        if (!user.UserId.HasValue)
        {
            return false;
        }

        var rightsOfUserInEvent = await cache.GetRightsOfUserInPartition(user.UserId.Value, eventId);
        return rightsOfUserInEvent.Contains(requestTypeName);
    }
}