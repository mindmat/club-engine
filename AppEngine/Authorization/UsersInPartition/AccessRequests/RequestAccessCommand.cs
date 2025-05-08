using System.Security.Authentication;

using AppEngine.Authentication.Users;
using AppEngine.DataAccess;
using AppEngine.ReadModels;
using AppEngine.TimeHandling;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Authorization.UsersInPartition.AccessRequests;

public class RequestAccessCommand : IRequest<Guid>
{
    public Guid PartitionId { get; set; }
    public string? RequestText { get; set; }
}

public class RequestAccessCommandHandler(IRepository<AccessToPartitionRequest> accessRequests,
                                         IRepository<User> users,
                                         AuthenticatedUserId authenticatedUserId,
                                         IAuthenticatedUserProvider authenticatedUserProvider,
                                         ChangeTrigger changeTrigger,
                                         RequestTimeProvider timeProvider)
    : IRequestHandler<RequestAccessCommand, Guid>
{
    public async Task<Guid> Handle(RequestAccessCommand command, CancellationToken cancellationToken)
    {
        var userId = authenticatedUserId.UserId;

        if (userId != null)
        {
            var existingRequest = await accessRequests.Where(req => req.PartitionId == command.PartitionId
                                                                 && req.UserId_Requestor == userId
                                                                 && (req.Response == null || req.Response == RequestResponse.Granted))
                                                      .FirstOrDefaultAsync(cancellationToken);

            if (existingRequest != null)
            {
                return existingRequest.Id;
            }
        }
        else
        {
            var authenticatedUser = authenticatedUserProvider.GetAuthenticatedUser();

            if (authenticatedUser.IdentityProviderUserIdentifier == null)
            {
                throw new AuthenticationException("You are not authenticated");
            }

            var newUser = new User
                          {
                              Id = Guid.NewGuid(),
                              IdentityProvider = authenticatedUser.IdentityProvider,
                              IdentityProviderUserIdentifier = authenticatedUser.IdentityProviderUserIdentifier,
                              FirstName = authenticatedUser.FirstName,
                              LastName = authenticatedUser.LastName,
                              Email = authenticatedUser.Email,
                              AvatarUrl = authenticatedUser.AvatarUrl
                          };
            users.Insert(newUser);
            userId = newUser.Id;

            // some user info is not present in the token, but exposed through an API
            changeTrigger.EnqueueCommand(new UpdateUserInfoCommand
                                         {
                                             Provider = authenticatedUser.IdentityProvider,
                                             Identifier = authenticatedUser.IdentityProviderUserIdentifier
                                         });
        }

        var request = new AccessToPartitionRequest
                      {
                          Id = Guid.NewGuid(),
                          UserId_Requestor = userId.Value,
                          RequestText = command.RequestText,
                          PartitionId = command.PartitionId,
                          RequestReceived = timeProvider.RequestNow
                      };
        accessRequests.Insert(request);

        changeTrigger.GlobalQueryChanged<MyPartitionsQuery>(userId.Value);
        changeTrigger.QueryChanged<AccessRequestsOfPartitionQuery>(command.PartitionId);

        return request.Id;
    }
}