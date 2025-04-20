using AppEngine.Authentication.Users;
using AppEngine.DataAccess;
using AppEngine.DomainEvents;
using AppEngine.ReadModels;
using AppEngine.ServiceBus;
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
                                         AuthenticatedUserId _user,
                                         IAuthenticatedUserProvider authenticatedUserProvider,
                                         IEventBus eventBus,
                                         CommandQueue commandQueue,
                                         RequestTimeProvider timeProvider)
    : IRequestHandler<RequestAccessCommand, Guid>
{
    public async Task<Guid> Handle(RequestAccessCommand command, CancellationToken cancellationToken)
    {
        var requestExpression = accessRequests.Where(req => req.PartitionId == command.PartitionId
                                                         && (req.Response == null || req.Response == RequestResponse.Granted));
        var existingUserId = await authenticatedUserProvider.GetAuthenticatedUserId();

        if (existingUserId != null)
        {
            requestExpression = requestExpression.Where(req => req.UserId_Requestor == existingUserId);
        }
        else
        {
            var authenticatedUser = authenticatedUserProvider.GetAuthenticatedUser();

            if (authenticatedUser.IdentityProviderUserIdentifier == null)
            {
                throw new ArgumentException("You are not authenticated");
            }

            requestExpression = requestExpression.Where(req => req.IdentityProvider == authenticatedUser.IdentityProvider
                                                            && req.Identifier == authenticatedUser.IdentityProviderUserIdentifier);
        }

        var request = await requestExpression.FirstOrDefaultAsync(cancellationToken);

        if (request == null)
        {
            var user = authenticatedUserProvider.GetAuthenticatedUser();

            request = new AccessToPartitionRequest
                      {
                          Id = Guid.NewGuid(),
                          UserId_Requestor = _user.UserId,
                          IdentityProvider = user.IdentityProvider,
                          Identifier = user.IdentityProviderUserIdentifier,
                          FirstName = user.FirstName,
                          LastName = user.LastName,
                          Email = user.Email,
                          AvatarUrl = user.AvatarUrl,
                          RequestText = command.RequestText,
                          PartitionId = command.PartitionId,
                          RequestReceived = timeProvider.RequestNow
                      };
            accessRequests.Insert(request);

            eventBus.Publish(new QueryChanged
                             {
                                 QueryName = nameof(PartitionsOfUserQuery)
                             });

            eventBus.Publish(new QueryChanged
                             {
                                 PartitionId = command.PartitionId,
                                 QueryName = nameof(AccessRequestsOfPartitionQuery)
                             });

            if (string.IsNullOrEmpty(user.FirstName)
             || string.IsNullOrEmpty(user.LastName)
             || string.IsNullOrEmpty(user.Email))
            {
                commandQueue.EnqueueCommand(new UpdateUserInfoCommand
                                            {
                                                Provider = request.IdentityProvider,
                                                Identifier = request.Identifier
                                            });
            }
        }

        return request.Id;
    }
}