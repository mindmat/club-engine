using AppEngine.Authorization.UsersInPartition.AccessRequests;
using AppEngine.DataAccess;
using AppEngine.Types;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Authentication.Users;

public class UpdateUserInfoCommand : IRequest
{
    public IdentityProvider Provider { get; set; }
    public string? Identifier { get; set; } = null!;
}

public class UpdateUserInfoCommandHandler(IIdentityProvider identityProvider,
                                          IRepository<AccessToPartitionRequest> accessRequests,
                                          IRepository<User> _users)
    : IRequestHandler<UpdateUserInfoCommand>
{
    public async Task Handle(UpdateUserInfoCommand command, CancellationToken cancellationToken)
    {
        var users = await _users.AsTracking()
                                .Where(usr => usr.IdentityProvider == command.Provider
                                           && (usr.FirstName == null || usr.LastName == null || usr.Email == null || usr.AvatarUrl == null))
                                .WhereIf(command.Identifier != null, usr => usr.IdentityProviderUserIdentifier == command.Identifier)
                                .ToListAsync(cancellationToken);

        if (users.Count == 0)
        {
            return;
        }

        var identifiers = users.Select(usr => usr.IdentityProviderUserIdentifier)
                               .WhereNotNull()
                               .Distinct();

        foreach (var identifier in identifiers)
        {
            var userDetails = await identityProvider.GetUserDetails(identifier!);

            if (userDetails != null)
            {
                users.Where(usr => usr.IdentityProviderUserIdentifier == identifier)
                     .ForEach(usr =>
                     {
                         usr.FirstName = userDetails.FirstName ?? usr.FirstName;
                         usr.LastName = userDetails.LastName ?? usr.LastName;
                         usr.Email = userDetails.Email ?? usr.Email;
                         usr.AvatarUrl = userDetails.AvatarUrl ?? usr.AvatarUrl;
                     });
            }
        }
    }
}