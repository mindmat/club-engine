using AppEngine.Authorization.UsersInPartition;

using MediatR;

namespace AppEngine.Authorization;

public class MyRightsInPartitionQuery : IRequest<IEnumerable<string>>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
}

public class MyRightsInPartitionQueryHandler(AuthenticatedUserId userId,
                                             RightsOfUserInPartitionCache cache)
    : IRequestHandler<MyRightsInPartitionQuery, IEnumerable<string>>
{
    public async Task<IEnumerable<string>> Handle(MyRightsInPartitionQuery query, CancellationToken cancellationToken)
    {
        return userId.UserId == null
            ? []
            : await cache.GetRightsOfUserInPartition(userId.UserId.Value, query.PartitionId);
    }
}