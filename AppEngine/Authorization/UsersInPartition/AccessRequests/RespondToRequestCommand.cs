using AppEngine.Authentication.Users;
using AppEngine.DataAccess;
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
                                            ChangeTrigger changeTrigger)
    : IRequestHandler<RespondToRequestCommand>
{
    public async Task Handle(RespondToRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await accessRequests.FirstAsync(req => req.Id == command.AccessToEventRequestId,
                                                      cancellationToken);

        if (request.Response != null)
        {
            throw new ArgumentException("Request has already been answered");
        }

        request.Response = command.Response;

        if (request.Response == RequestResponse.Granted)
        {
            var userInEvent = new UserInPartition
                              {
                                  Id = Guid.NewGuid(),
                                  UserId = request.UserId_Requestor,
                                  PartitionId = request.PartitionId,
                                  Role = UserInPartitionRole.Reader
                              };
            usersInPartitions.Insert(userInEvent);
        }

        changeTrigger.QueryChanged<UsersOfPartitionQuery>(command.PartitionId);
        changeTrigger.QueryChanged<AccessRequestsOfPartitionQuery>(command.PartitionId);
    }
}