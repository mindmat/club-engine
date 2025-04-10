﻿using AppEngine.Authentication;
using AppEngine.Authentication.Users;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AppEngine.Authorization.UsersInPartition;

internal class AuthenticatedUserProvider(IHttpContextAccessor httpContextAccessor,
                                         IIdentityProvider identityProvider,
                                         IQueryable<User> users)
    : IAuthenticatedUserProvider
{
    public AuthenticatedUser GetAuthenticatedUser()
    {
        return identityProvider.GetUser(httpContextAccessor);
    }

    public async Task<Guid?> GetAuthenticatedUserId()
    {
        var identifier = identityProvider.GetIdentifier(httpContextAccessor);
        if (identifier != null)
        {
            return await users.Where(usr => usr.IdentityProvider == identifier.Value.Provider
                                         && usr.IdentityProviderUserIdentifier == identifier.Value.Identifier)
                              .Select(usr => (Guid?)usr.Id)
                              .FirstOrDefaultAsync();
        }

        return null;
    }
}