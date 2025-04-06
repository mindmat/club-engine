namespace AppEngine.Authorization.UsersInPartition;

public class AuthenticatedUserId(Guid? userId)
{
    public Guid? UserId { get; } = userId;
}