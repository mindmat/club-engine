using AppEngine.Json;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.ReadModels;

public class ReadModelReader(IQueryable<QueryReadModel> _readModels, Serializer _serializer)
{
    public async Task<SerializedJson<T>> Get<T>(string queryName,
                                                Guid eventId,
                                                Guid? rowId,
                                                CancellationToken cancellationToken)
        where T : class
    {
        var readModel = await _readModels.Where(rdm => rdm.QueryName == queryName
                                                    && rdm.EventId == eventId
                                                    && rdm.RowId == rowId)
                                         .Select(rdm => rdm.ContentJson)
                                         .FirstAsync(cancellationToken);
        return new SerializedJson<T>(readModel);
    }

    public async Task<T> GetDeserialized<T>(string queryName,
                                            Guid eventId,
                                            Guid? rowId,
                                            CancellationToken cancellationToken)
    {
        var readModel = await _readModels.Where(rdm => rdm.QueryName == queryName
                                                    && rdm.EventId == eventId
                                                    && rdm.RowId == rowId)
                                         .Select(rdm => rdm.ContentJson)
                                         .FirstAsync(cancellationToken);
        return _serializer.Deserialize<T>(readModel)!;
    }

    public async Task<IEnumerable<T>> GetDeserialized<T>(string queryName,
                                                         Guid eventId,
                                                         IEnumerable<Guid> rowIds,
                                                         CancellationToken cancellationToken)
    {
        var readModels = await _readModels.Where(rdm => rdm.QueryName == queryName
                                                     && rdm.EventId == eventId
                                                     && rdm.RowId != null
                                                     && rowIds.Contains(rdm.RowId.Value))
                                          .Select(rdm => rdm.ContentJson)
                                          .ToListAsync(cancellationToken);
        return readModels.Select(rmd => _serializer.Deserialize<T>(rmd)!);
    }
}