using AppEngine.Authorization.UsersInPartition;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AppEngine.Authorization;

public class RightsOfUserInPartitionCache(IQueryable<UserInPartition> usersInPartitions,
                                      IMemoryCache memoryCache,
                                      IRightsOfPartitionRoleProvider rightsOfPartitionRoleProvider)
{
    private readonly TimeSpan _slidingExpiration = new(0, 1, 0, 0);


    public async Task<HashSet<string>> GetRightsOfUserInPartition(Guid userId, Guid partitionId)
    {
        var key = new UserInPartitionCacheKey(userId, partitionId);
        return await memoryCache.GetOrCreateAsync(key, entry => CreateRightsOfUserInPartitionCacheEntry(entry, userId, partitionId));
    }

    private async Task<HashSet<string>> CreateRightsOfUserInPartitionCacheEntry(ICacheEntry entry, Guid userId, Guid partitionId)
    {
        entry.SlidingExpiration = _slidingExpiration;
        var usersRolesInPartition = await usersInPartitions.Where(uie => uie.UserId == userId
                                                                      && uie.PartitionId == partitionId)
                                                           .Select(uie => uie.Role)
                                                           .ToListAsync();

        return rightsOfPartitionRoleProvider.GetRightsOfPartitionRoles(partitionId, usersRolesInPartition)
                                        .OrderBy(rgt => rgt)
                                        .ToHashSet();
    }
}

public class UserInPartitionCacheKey(Guid userId, Guid partitionId)
{
    private readonly Guid _partitionId = partitionId;
    private readonly Guid _userId = userId;

    public override bool Equals(object? obj)
    {
        return obj is UserInPartitionCacheKey key && _userId.Equals(key._userId) && _partitionId.Equals(key._partitionId);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_userId, _partitionId);
    }
}