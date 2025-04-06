using AppEngine.Authorization.UsersInPartition;

namespace AppEngine.Authorization;


public interface IRightsOfPartitionRoleProvider
{
    IEnumerable<string> GetRightsOfPartitionRoles(Guid partitionId, ICollection<UserInPartitionRole> usersPartitionsInEvent);
}

internal class RightsOfPartitionRoleProvider : IRightsOfPartitionRoleProvider
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
            yield return nameof(RightsOfUserInPartitionQuery);
        }

        if (usersRolesInEvent.Contains(UserInPartitionRole.Writer)
         || usersRolesInEvent.Contains(UserInPartitionRole.Admin))
        {

        }

        if (usersRolesInEvent.Contains(UserInPartitionRole.Admin))
        {
            yield return nameof(SetRoleOfUserInPartitionCommand);
            yield return nameof(RemoveUserFromPartitionCommand);

        }
    }
}