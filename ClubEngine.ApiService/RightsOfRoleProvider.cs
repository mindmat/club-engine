using AppEngine.Authorization;
using AppEngine.Authorization.UsersInPartition;

using ClubEngine.ApiService.Clubs;
using ClubEngine.ApiService.Members;
using ClubEngine.ApiService.Members.Import;
using ClubEngine.ApiService.Members.Memberships;
using ClubEngine.ApiService.MembershipFees;
using ClubEngine.ApiService.Slack;

namespace ClubEngine.ApiService;

internal class RightsOfRoleProvider : IRightsOfPartitionRoleProvider
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
            yield return nameof(MemberQuery);
            yield return nameof(MemberStatsQuery);
            yield return nameof(MembershipFeesQuery);
            yield return nameof(PeriodsQuery);
        }

        if (usersRolesInEvent.Contains(UserInPartitionRole.Writer)
         || usersRolesInEvent.Contains(UserInPartitionRole.Admin))
        {
            yield return nameof(ImportMemberListQuery);
            yield return nameof(ImportNewMembersCommand);
            yield return nameof(SlackUserDifferencesQuery);
            yield return nameof(MapSlackUserCommand);
            yield return nameof(RemoveSlackUserMappingCommand);
            yield return nameof(UpsertMembershipFeesForPeriodCommand);
        }

        if (usersRolesInEvent.Contains(UserInPartitionRole.Admin))
        {
        }
    }
}