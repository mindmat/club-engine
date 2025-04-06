using AppEngine.Authentication;

namespace AppEngine.Authorization.UsersInPartition;

public class AuthenticatedUser(IdentityProvider identityProvider,
                               string identityProviderUserIdentifier,
                               string? firstName = null,
                               string? lastName = null,
                               string? email = null,
                               string? avatarUrl = null)
{
    public IdentityProvider IdentityProvider { get; } = identityProvider;
    public string IdentityProviderUserIdentifier { get; } = identityProviderUserIdentifier;
    public string? FirstName { get; } = firstName;
    public string? LastName { get; } = lastName;
    public string? Email { get; } = email;
    public string? AvatarUrl { get; } = avatarUrl;

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