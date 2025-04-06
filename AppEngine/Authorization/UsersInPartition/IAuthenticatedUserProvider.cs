namespace AppEngine.Authorization.UsersInPartition;

public interface IAuthenticatedUserProvider
{
    AuthenticatedUser GetAuthenticatedUser();
    Task<Guid?> GetAuthenticatedUserId();
}