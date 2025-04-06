using AppEngine.Authentication.Users;
using AppEngine.DataAccess;
using AppEngine.DomainEvents;
using AppEngine.ReadModels;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Authorization.UsersInPartition.AccessRequests;

public class RespondToRequestCommand : IRequest, IPartitionBoundRequest
{
    public Guid AccessToEventRequestId { get; set; }
    public Guid PartitionId { get; set; }
    public RequestResponse Response { get; set; }
}

public class RespondToRequestCommandHandler(IRepository<AccessToPartitionRequest> accessRequests,
                                            IRepository<UserInPartition> usersInPartitions,
                                            IRepository<User> users,
                                            IEventBus eventBus)
    : IRequestHandler<RespondToRequestCommand>
{
    public async Task Handle(RespondToRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await accessRequests.FirstAsync(req => req.Id == command.AccessToEventRequestId, cancellationToken);
        if (request.Response != null)
        {
            throw new ArgumentException("Request has already been answered");
        }

        request.Response = command.Response;
        if (request.Response == RequestResponse.Granted)
        {
            var userId = await CreateUserIfNecessary(request);
            var userInEvent = new UserInPartition
                              {
                                  Id = Guid.NewGuid(),
                                  UserId = userId,
                                  PartitionId = request.PartitionId,
                                  Role = UserInPartitionRole.Reader
                              };
            usersInPartitions.Insert(userInEvent);
        }

        eventBus.Publish(new QueryChanged
                         {
                             PartitionId = command.PartitionId,
                             QueryName = nameof(UsersOfPartitionQuery)
                         });
        eventBus.Publish(new QueryChanged
                         {
                             PartitionId = command.PartitionId,
                             QueryName = nameof(AccessRequestsOfPartitionQuery)
                         });
    }

    private async Task<Guid> CreateUserIfNecessary(AccessToPartitionRequest request)
    {
        if (request.UserId_Requestor.HasValue)
        {
            return request.UserId_Requestor.Value;
        }
        // ToDo: check if user already exists
        var user = new User
                   {
                       Id = Guid.NewGuid(),
                       IdentityProvider = request.IdentityProvider,
                       IdentityProviderUserIdentifier = request.Identifier,
                       FirstName = request.FirstName,
                       LastName = request.LastName,
                       Email = request.Email
                   };
        users.Insert(user);

        return user.Id;
    }
}