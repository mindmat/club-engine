using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Authorization.UsersInPartition.AccessRequests;

public class AccessRequestsOfPartitionQuery : IRequest<IEnumerable<AccessRequestOfPartition>>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public bool IncludeDeniedRequests { get; set; }
}

public class AccessRequestOfPartition
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTimeOffset RequestReceived { get; set; }
    public string? RequestText { get; set; }
}

public class AccessRequestsOfPartitionQueryHandler(IQueryable<AccessToPartitionRequest> accessRequests) 
    : IRequestHandler<AccessRequestsOfPartitionQuery, IEnumerable<AccessRequestOfPartition>>
{
    public async Task<IEnumerable<AccessRequestOfPartition>> Handle(AccessRequestsOfPartitionQuery query,
                                                                    CancellationToken cancellationToken)
    {
        return await accessRequests.Where(req => req.PartitionId == query.PartitionId)
                                   .Where(req => req.Response == null
                                                 || (req.Response == RequestResponse.Denied && query.IncludeDeniedRequests))
                                   .Select(req => new AccessRequestOfPartition
                                   {
                                       Id = req.Id,
                                       FirstName = req.FirstName,
                                       LastName = req.LastName,
                                       Email = req.Email,
                                       AvatarUrl = req.AvatarUrl,
                                       RequestReceived = req.RequestReceived,
                                       RequestText = req.RequestText
                                   })
                                   .ToListAsync(cancellationToken);
    }
}