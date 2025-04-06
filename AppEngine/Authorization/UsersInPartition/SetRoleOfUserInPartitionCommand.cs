using AppEngine.DataAccess;
using AppEngine.DomainEvents;
using AppEngine.ReadModels;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Authorization.UsersInPartition;

public class SetRoleOfUserInPartitionCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public Guid UserId { get; set; }
    public UserInPartitionRole Role { get; set; }
}

public class SetRoleOfUserInPartitionCommandHandler(IRepository<UserInPartition> usersInPartitions,
                                                    IEventBus eventBus)
    : IRequestHandler<SetRoleOfUserInPartitionCommand>
{
    public async Task Handle(SetRoleOfUserInPartitionCommand command, CancellationToken cancellationToken)
    {
        var userInPartition = await usersInPartitions.AsTracking()
                                                     .FirstOrDefaultAsync(uie => uie.PartitionId == command.PartitionId
                                                                                 && uie.UserId == command.UserId,
                                                                          cancellationToken);

        if (userInPartition == null)
        {
            usersInPartitions.Insert(new UserInPartition
            {
                Id = Guid.NewGuid(),
                PartitionId = command.PartitionId,
                UserId = command.UserId
            });
        }
        else
        {
            userInPartition.Role = command.Role;
        }

        eventBus.Publish(new QueryChanged
        {
            PartitionId = command.PartitionId,
            QueryName = nameof(UsersOfPartitionQuery)
        });
    }
}