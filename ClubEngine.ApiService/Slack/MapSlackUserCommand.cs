using AppEngine.Authorization;
using AppEngine.DataAccess;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.Slack;

public class MapSlackUserCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public required string SlackUserId { get; set; }
    public Guid MemberId { get; set; }
}

public class MapSlackUserCommandHandler(IRepository<SlackUserMapping> slackUserMappings)
    : IRequestHandler<MapSlackUserCommand>
{
    public async Task Handle(MapSlackUserCommand command, CancellationToken cancellationToken)
    {
        await slackUserMappings.Upsert(sum => sum.Member!.ClubId == command.PartitionId
                                           && sum.SlackUserId == command.SlackUserId,
                                       () => new SlackUserMapping
                                             {
                                                 Id = Guid.NewGuid(),
                                                 SlackUserId = command.SlackUserId
                                             },
                                       mapping => mapping.MemberId = command.MemberId,
                                       cancellationToken);
    }
}