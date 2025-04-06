using Microsoft.EntityFrameworkCore;

namespace AppEngine.Partitions;

public interface IPartitionAcronymResolver
{
    Task<Guid> GetPartitionIdFromAcronym(string partitionAcronym);
}

internal class PartitionAcronymResolver(IQueryable<IPartition> partitions) : IPartitionAcronymResolver
{
    public async Task<Guid> GetPartitionIdFromAcronym(string partitionAcronym)
    {
        var partition = await partitions.FirstOrDefaultAsync(ptt => ptt.Acronym == partitionAcronym);
        if (partition == null)
        {
            throw new ArgumentOutOfRangeException($"There is no partition {partitionAcronym}");
        }

        return partition.Id;
    }
}