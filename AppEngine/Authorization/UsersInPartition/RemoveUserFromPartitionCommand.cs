using AppEngine.DataAccess;
using AppEngine.DomainEvents;
using AppEngine.ReadModels;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Authorization.UsersInPartition;

public class RemoveUserFromPartitionCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public Guid UserId { get; set; }
}

public class RemoveUserFromPartitionCommandHandler(IRepository<UserInPartition> usersInPartitions,
                                                   IEventBus eventBus)
    : IRequestHandler<RemoveUserFromPartitionCommand>
{
    public async Task Handle(RemoveUserFromPartitionCommand command, CancellationToken cancellationToken)
    {
        var userInPartition = await usersInPartitions.FirstOrDefaultAsync(uie => uie.PartitionId == command.PartitionId
                                                                              && uie.UserId == command.UserId,
                                                                          cancellationToken);

        if (userInPartition != null)
        {
            usersInPartitions.Remove(userInPartition);
        }

        eventBus.Publish(new QueryChanged
        {
            PartitionId = command.PartitionId,
            QueryName = nameof(UsersOfPartitionQuery)
        });
    }
}