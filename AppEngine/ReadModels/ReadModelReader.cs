using System.Diagnostics.CodeAnalysis;

using AppEngine.Json;
using AppEngine.ServiceBus;
using AppEngine.TimeHandling;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.ReadModels;

public class ReadModelReader(IQueryable<QueryReadModel> _readModels,
                             Serializer serializer,
                             CommandQueue commandQueue,
                             RequestTimeProvider timeProvider)
{
    public async Task<SerializedJson<T>> Get<T>(string queryName,
                                                Guid partitionId,
                                                Guid? rowId,
                                                CancellationToken cancellationToken)
        where T : class
    {
        var readModel = await _readModels.Where(rdm => rdm.QueryName == queryName
                                                    && rdm.PartitionId == partitionId
                                                    && rdm.RowId == rowId)
                                         .Select(rdm => rdm.ContentJson)
                                         .FirstOrDefaultAsync(cancellationToken);

        HandleReadModelNotCreated(readModel, queryName, partitionId, rowId);

        return new SerializedJson<T>(readModel);
    }

    public async Task<T> GetDeserialized<T>(string queryName,
                                            Guid partitionId,
                                            Guid? rowId,
                                            CancellationToken cancellationToken)
    {
        var readModel = await _readModels.Where(rdm => rdm.QueryName == queryName
                                                    && rdm.PartitionId == partitionId
                                                    && rdm.RowId == rowId)
                                         .Select(rdm => rdm.ContentJson)
                                         .FirstOrDefaultAsync(cancellationToken);

        HandleReadModelNotCreated(readModel, queryName, partitionId, rowId);

        return serializer.Deserialize<T>(readModel)!;
    }

    public async Task<IEnumerable<T>> GetDeserialized<T>(string queryName,
                                                         Guid partitionId,
                                                         IEnumerable<Guid> rowIds,
                                                         CancellationToken cancellationToken)
    {
        var readModels = await _readModels.Where(rdm => rdm.QueryName == queryName
                                                     && rdm.PartitionId == partitionId
                                                     && rdm.RowId != null
                                                     && rowIds.Contains(rdm.RowId.Value))
                                          .Select(rdm => rdm.ContentJson)
                                          .ToListAsync(cancellationToken);

        return readModels.Select(rmd => serializer.Deserialize<T>(rmd)!);
    }

    private void HandleReadModelNotCreated([NotNull] string? readModel, string queryName, Guid partitionId, Guid? rowId)
    {
        if (readModel == null)
        {
            commandQueue.EnqueueCommand(new UpdateReadModelCommand
                                        {
                                            QueryName = queryName,
                                            PartitionId = partitionId,
                                            RowId = rowId,
                                            DirtyMoment = timeProvider.RequestNow
                                        },
                                        true);

            throw new InvalidOperationException($"Read model {queryName} not found for partition {partitionId} and row {rowId}");
        }
    }
}