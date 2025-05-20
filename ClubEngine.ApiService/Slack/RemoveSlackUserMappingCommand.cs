using AppEngine.Authorization;
using AppEngine.DataAccess;

using MediatR;

namespace ClubEngine.ApiService.Slack;

public class RemoveSlackUserMappingCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public required string SlackUserId { get; set; }
    public Guid MemberId { get; set; }
}

public class RemoveSlackUserMappingCommandHandler(IRepository<SlackUserMapping> slackUserMappings)
    : IRequestHandler<RemoveSlackUserMappingCommand>
{
    public Task Handle(RemoveSlackUserMappingCommand command, CancellationToken cancellationToken)
    {
        slackUserMappings.Remove(sum => sum.Member!.ClubId == command.PartitionId
                                     && sum.SlackUserId == command.SlackUserId
                                     && sum.MemberId == command.MemberId);

        return Task.CompletedTask;
    }
}