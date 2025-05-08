using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Authorization.UsersInPartition.AccessRequests;

public class AccessRequestsOfPartitionQuery : IRequest<IEnumerable<AccessRequestOfPartition>>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public bool IncludeDeniedRequests { get; set; }
}

public record AccessRequestOfPartition(Guid Id,
                                       string? FirstName,
                                       string? LastName,
                                       string? Email,
                                       string? AvatarUrl,
                                       DateTimeOffset RequestReceived,
                                       string? RequestText);

public class AccessRequestsOfPartitionQueryHandler(IQueryable<AccessToPartitionRequest> accessRequests)
    : IRequestHandler<AccessRequestsOfPartitionQuery, IEnumerable<AccessRequestOfPartition>>
{
    public async Task<IEnumerable<AccessRequestOfPartition>> Handle(AccessRequestsOfPartitionQuery query,
                                                                    CancellationToken cancellationToken)
    {
        return await accessRequests.Where(req => req.PartitionId == query.PartitionId
                                              && (req.Response == null
                                               || (req.Response == RequestResponse.Denied && query.IncludeDeniedRequests)))
                                   .Select(req => new AccessRequestOfPartition(req.Id,
                                                                               req.User_Requestor!.FirstName,
                                                                               req.User_Requestor.LastName,
                                                                               req.User_Requestor.Email,
                                                                               req.User_Requestor.AvatarUrl,
                                                                               req.RequestReceived,
                                                                               req.RequestText))
                                   .ToListAsync(cancellationToken);
    }
}