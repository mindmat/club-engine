using AppEngine.Authorization.UsersInPartition;
using AppEngine.Authorization.UsersInPartition.AccessRequests;
using AppEngine.MenuNodes;

namespace AppEngine.Authorization;

public interface IRightsOfPartitionRoleProvider
{
    IEnumerable<string> GetRightsOfPartitionRoles(Guid partitionId, ICollection<UserInPartitionRole> usersPartitionsInEvent);
}

internal class AppEngineRightsOfPartitionRoleProvider : IRightsOfPartitionRoleProvider
{
    /// <summary>
    /// hook for dynamic setup of rights in roles per event
    /// hardcoded to start
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetRightsOfPartitionRoles(Guid partitionId, ICollection<UserInPartitionRole> usersRolesInEvent)
    {
        if (usersRolesInEvent.Contains(UserInPartitionRole.Reader)
         || usersRolesInEvent.Contains(UserInPartitionRole.Writer)
         || usersRolesInEvent.Contains(UserInPartitionRole.Admin))
        {
            yield return nameof(MyRightsInPartitionQuery);
            yield return nameof(UsersOfPartitionQuery);
            yield return nameof(MenuNodesQuery);
        }

        if (usersRolesInEvent.Contains(UserInPartitionRole.Writer)
         || usersRolesInEvent.Contains(UserInPartitionRole.Admin))
        {
        }

        if (usersRolesInEvent.Contains(UserInPartitionRole.Admin))
        {
            yield return nameof(AccessRequestsOfPartitionQuery);
            yield return nameof(RespondToRequestCommand);
            yield return nameof(SetRoleOfUserInPartitionCommand);
            yield return nameof(RemoveUserFromPartitionCommand);
        }
    }
}