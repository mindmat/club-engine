using AppEngine.Accounting.Account;
using AppEngine.Accounting.Assignments;
using AppEngine.Accounting.Bookings;
using AppEngine.Accounting.Iso20022.Camt;
using AppEngine.Authorization.UsersInPartition;
using AppEngine.Authorization.UsersInPartition.AccessRequests;
using AppEngine.Mailing.Configuration;
using AppEngine.MenuNodes;
using AppEngine.ReadModels;

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
            yield return nameof(PaymentsByDayQuery);
            yield return nameof(UpdateReadModelCommand);
        }

        if (usersRolesInEvent.Contains(UserInPartitionRole.Writer)
         || usersRolesInEvent.Contains(UserInPartitionRole.Admin))
        {
            yield return nameof(SavePaymentFileCommand);
            yield return nameof(PaymentAssignmentsQuery);
            yield return nameof(AssignIncomingPaymentCommand);
            yield return nameof(UnassignPaymentCommand);
        }

        if (usersRolesInEvent.Contains(UserInPartitionRole.Admin))
        {
            yield return nameof(AccessRequestsOfPartitionQuery);
            yield return nameof(RespondToRequestCommand);
            yield return nameof(SetRoleOfUserInPartitionCommand);
            yield return nameof(RemoveUserFromPartitionCommand);

            yield return nameof(ExternalMailConfigurationQuery);
            yield return nameof(SaveExternalMailConfigurationCommand);
            yield return nameof(BankAccountConfigurationQuery);
            yield return nameof(SaveBankAccountConfigurationCommand);
            yield return nameof(UsersOfPartitionQuery);
        }
    }
}