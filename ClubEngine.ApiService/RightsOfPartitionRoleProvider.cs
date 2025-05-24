using AppEngine.Authorization;
using AppEngine.Authorization.UsersInPartition;

using ClubEngine.ApiService.Members;
using ClubEngine.ApiService.Members.Import;
using ClubEngine.ApiService.Members.Memberships;
using ClubEngine.ApiService.Slack;

namespace ClubEngine.ApiService;

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
            yield return nameof(MembershipTypesQuery);
            yield return nameof(MembersQuery);
            yield return nameof(MemberStatsQuery);
        }

        if (usersRolesInEvent.Contains(UserInPartitionRole.Writer)
         || usersRolesInEvent.Contains(UserInPartitionRole.Admin))
        {
            yield return nameof(ImportMemberListQuery);
            yield return nameof(ImportNewMembersCommand);
            yield return nameof(SlackUserDifferencesQuery);
            yield return nameof(MapSlackUserCommand);
            yield return nameof(RemoveSlackUserMappingCommand);
        }

        if (usersRolesInEvent.Contains(UserInPartitionRole.Admin))
        {
        }
    }
}