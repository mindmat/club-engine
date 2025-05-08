using AppEngine.Authentication;

namespace AppEngine.Authorization.UsersInPartition;

public record AuthenticatedUser(IdentityProvider IdentityProvider,
                                string IdentityProviderUserIdentifier,
                                string? FirstName = null,
                                string? LastName = null,
                                string? Email = null,
                                string? AvatarUrl = null)
{
    public string GetText()
    {
        if (!string.IsNullOrWhiteSpace(FirstName)
         || !string.IsNullOrWhiteSpace(LastName))
        {
            return $"{FirstName} {LastName}";
        }

        if (!string.IsNullOrWhiteSpace(Email))
        {
            return Email;
        }

        return IdentityProviderUserIdentifier;
    }

    public static AuthenticatedUser None => new(0, string.Empty);
}