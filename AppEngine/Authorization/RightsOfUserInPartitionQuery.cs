using AppEngine.Authorization.UsersInPartition;

using MediatR;

namespace AppEngine.Authorization;

public class RightsOfUserInPartitionQuery : IRequest<IEnumerable<string>>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
}

public class RightsOfUserInPartitionQueryHandler(AuthenticatedUserId userId,
                                                 RightsOfUserInPartitionCache cache)
    : IRequestHandler<RightsOfUserInPartitionQuery, IEnumerable<string>>
{
    public async Task<IEnumerable<string>> Handle(RightsOfUserInPartitionQuery query, CancellationToken cancellationToken)
    {
        return userId.UserId == null
            ? Enumerable.Empty<string>()
            : await cache.GetRightsOfUserInPartition(userId.UserId.Value, query.PartitionId);
    }
}