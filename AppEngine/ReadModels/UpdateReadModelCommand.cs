using AppEngine.DataAccess;
using AppEngine.Json;
using AppEngine.MenuNodes;
using AppEngine.TimeHandling;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.ReadModels;

public class UpdateReadModelCommand : IRequest
{
    public Guid PartitionId { get; set; }
    public string QueryName { get; set; } = null!;
    public Guid? RowId { get; set; }
    public DateTimeOffset? DirtyMoment { get; set; }
}

public class UpdateReadModelCommandHandler(IEnumerable<IReadModelCalculator> calculators,
                                           DbContext dbContext,
                                           ChangeTrigger changeTrigger,
                                           RequestTimeProvider dateTimeProvider,
                                           Serializer serializer,
                                           IRepository<MenuNodeReadModel> menuNodes)
    : IRequestHandler<UpdateReadModelCommand>
{
    public async Task Handle(UpdateReadModelCommand command, CancellationToken cancellationToken)
    {
        var updater = calculators.First(rmu => rmu.QueryName == command.QueryName);

        var readModels = dbContext.Set<QueryReadModel>();

        var readModel = await readModels.AsTracking()
                                        .Where(rdm => rdm.QueryName == command.QueryName
                                                   && rdm.PartitionId == command.PartitionId
                                                   && rdm.RowId == command.RowId)
                                        .FirstOrDefaultAsync(cancellationToken);

        if (readModel?.LastUpdate >= command.DirtyMoment)
        {
            // Not perfect (time vs row version), but still servers as debouncing
            return;
        }

        var result = await updater.Calculate(command.PartitionId, command.RowId, cancellationToken);

        UpsertReadModel(command, result.ReadModel, readModel, readModels);

        if (result.MenuNode != null)
        {
            await UpsertMenuNode(command.PartitionId, result.MenuNode);
        }
    }

    private void UpsertReadModel(UpdateReadModelCommand command,
                                 object calculated,
                                 QueryReadModel? existing,
                                 DbSet<QueryReadModel> readModels)
    {
        var contentJson = serializer.Serialize(calculated);

        if (existing == null)
        {
            var node = new QueryReadModel
                       {
                           QueryName = command.QueryName,
                           PartitionId = command.PartitionId,
                           RowId = command.RowId,
                           ContentJson = contentJson,
                           LastUpdate = dateTimeProvider.RequestNow
                       };
            var entry = readModels.Attach(node);
            entry.State = EntityState.Added;

            changeTrigger.QueryChanged(command.QueryName,
                                       command.PartitionId,
                                       command.RowId);
        }
        else
        {
            existing.ContentJson = contentJson;
            var contentHasChanged = dbContext.Entry(existing).State == EntityState.Modified;

            if (contentHasChanged)
            {
                changeTrigger.QueryChanged(command.QueryName,
                                           command.PartitionId,
                                           command.RowId);
            }

            existing.LastUpdate = dateTimeProvider.RequestNow;
        }
    }

    private async Task UpsertMenuNode(Guid partitionId, MenuNodeCalculation menuNodeCalculation)
    {
        var node = await menuNodes.AsTracking()
                                  .FirstOrDefaultAsync(mnr => mnr.PartitionId == partitionId
                                                           && mnr.Key == menuNodeCalculation.Key);
        var anythingChanged = false;

        if (node == null)
        {
            anythingChanged = true;

            menuNodes.Insert(new MenuNodeReadModel
                             {
                                 Id = Guid.NewGuid(),
                                 PartitionId = partitionId,
                                 Key = menuNodeCalculation.Key,
                                 Content = menuNodeCalculation.Content,
                                 Hidden = menuNodeCalculation.Hidden
                             });
        }
        else if (node.Content != menuNodeCalculation.Content
              || node.Style != menuNodeCalculation.Style
              || node.Hidden != menuNodeCalculation.Hidden)
        {
            anythingChanged = true;
            node.Content = menuNodeCalculation.Content;
            node.Style = menuNodeCalculation.Style;
            node.Hidden = menuNodeCalculation.Hidden;
        }

        if (anythingChanged)
        {
            changeTrigger.QueryChanged<MenuNodesQuery>(partitionId);
        }
    }
}

public interface IReadModelCalculator
{
    string QueryName { get; }
    bool IsDateDependent { get; }
    Task<(object ReadModel, MenuNodeCalculation? MenuNode)> Calculate(Guid partitionId, Guid? rowId, CancellationToken cancellationToken);
}

public abstract class ReadModelCalculator<T> : IReadModelCalculator
    where T : class
{
    public abstract string QueryName { get; }
    public abstract bool IsDateDependent { get; }

    public async Task<(object ReadModel, MenuNodeCalculation? MenuNode)> Calculate(Guid partitionId, Guid? rowId, CancellationToken cancellationToken)
    {
        return await CalculateTyped(partitionId, rowId, cancellationToken);
    }

    protected abstract Task<(T ReadModel, MenuNodeCalculation? MenuNode)> CalculateTyped(Guid partitionId, Guid? rowId, CancellationToken cancellationToken);
}

public class MenuNodeCalculation
{
    public required string Key { get; set; }
    public string? Content { get; set; }
    public MenuNodeStyle Style { get; set; }
    public bool Hidden { get; set; }
}