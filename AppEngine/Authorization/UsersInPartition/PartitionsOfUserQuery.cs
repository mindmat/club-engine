using AppEngine.Authorization.UsersInPartition.AccessRequests;
using AppEngine.DataAccess;
using AppEngine.Internationalization;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Authorization.UsersInPartition;

public class PartitionsOfUserQuery : IRequest<PartitionsOfUser>;

public class PartitionsOfUserQueryHandler(IQueryable<UserInPartition> usersInPartitions,
                                          AuthenticatedUserId authenticatedUserId,
                                          AuthenticatedUser authenticatedUser,
                                          IQueryable<AccessToPartitionRequest> accessRequests,
                                          Translator translator)
    : IRequestHandler<PartitionsOfUserQuery, PartitionsOfUser>
{
    public async Task<PartitionsOfUser> Handle(PartitionsOfUserQuery request,
                                               CancellationToken cancellationToken)
    {
        if (authenticatedUserId.UserId == null && authenticatedUser == AuthenticatedUser.None)
        {
            return new PartitionsOfUser
            {
                AuthorizedPartitions = [],
                Requests = []
            };
        }

        return new PartitionsOfUser
        {
            AuthorizedPartitions = await usersInPartitions.Where(uip => uip.UserId == authenticatedUserId.UserId)
                                                          //.OrderBy(uip => uip.Partition!.State)
                                                          .OrderBy(uip => uip.Partition!.Name)
                                                          .Select(uip => new PartitionOfUser
                                                          {
                                                              EventId = uip.PartitionId,
                                                              EventName = uip.Partition!.Name,
                                                              EventAcronym = uip.Partition.Acronym,
                                                              //EventState = uip.Partition.State,
                                                              //EventStateText = translator.Translate(uip.Event.State),
                                                              Role = uip.Role,
                                                              RoleText = translator.TranslateEnum(uip.Role)
                                                          })
                                                          .ToListAsync(cancellationToken),

            Requests = await accessRequests.WhereIf(authenticatedUserId.UserId != null,
                                                    req => req.UserId_Requestor == authenticatedUserId.UserId)
                                           .WhereIf(authenticatedUserId.UserId == null && authenticatedUser != AuthenticatedUser.None,
                                                    req => req.IdentityProvider == authenticatedUser.IdentityProvider
                                                           && req.Identifier == authenticatedUser.IdentityProviderUserIdentifier)
                                           .Select(req => new AccessRequest
                                           {
                                               PartitionId = req.PartitionId,
                                               PartitionName = req.Partition!.Name,
                                               PartitionAcronym = req.Partition.Acronym,
                                               RequestSent = req.RequestReceived
                                           })
                                           .ToListAsync(cancellationToken)
        };
    }
}

public class PartitionsOfUser
{
    public IEnumerable<PartitionOfUser> AuthorizedPartitions { get; set; } = null!;
    public IEnumerable<AccessRequest> Requests { get; set; } = null!;
}

public class PartitionOfUser
{
    public Guid EventId { get; set; }
    public string EventName { get; set; } = null!;
    public string EventAcronym { get; set; } = null!;
    public UserInPartitionRole Role { get; set; }
    public string RoleText { get; set; } = null!;
}

public class AccessRequest
{
    public Guid PartitionId { get; set; }
    public string PartitionName { get; set; } = null!;
    public string PartitionAcronym { get; set; } = null!;
    public DateTimeOffset RequestSent { get; set; }
}