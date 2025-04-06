using AppEngine.DataAccess;
using AppEngine.Partitions;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.ReadModels;

public class StartUpdateReadModelsOfEventCommand : IRequest
{
    public Guid? PartitionId { get; set; }
    public IEnumerable<string>? QueryNames { get; set; }
}

public class StartUpdateReadModelsOfEventCommandHandler(IQueryable<IPartition> partitions,
                                                        ChangeTrigger changeTrigger)
    : IRequestHandler<StartUpdateReadModelsOfEventCommand>
{
    public async Task Handle(StartUpdateReadModelsOfEventCommand command, CancellationToken cancellationToken)
    {
        var eventIds = await partitions.WhereIf(command.PartitionId != null, evt => evt.Id == command.PartitionId)
                                       .Select(ptt => ptt.Id)
                                       .ToListAsync(cancellationToken);

        foreach (var eventId in eventIds)
        {
            //if (command.QueryNames?.Contains(nameof(PendingMailsQuery)) != false)
            //{
            //    changeTrigger.TriggerUpdate<PendingMailsCalculator>(null, eventId);
            //}
        }
    }
}