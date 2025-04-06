using AppEngine.Authentication.Users;
using AppEngine.Authorization.UsersInPartition;

using Microsoft.AspNetCore.Http;

namespace AppEngine.Authentication;

public interface IIdentityProvider
{
    (IdentityProvider Provider, string Identifier)? GetIdentifier(IHttpContextAccessor contextAccessor);
    AuthenticatedUser GetUser(IHttpContextAccessor httpContextAccessor);
    Task<ExternalUserDetails?> GetUserDetails(string identifier);
}