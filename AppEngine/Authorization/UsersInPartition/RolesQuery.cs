using AppEngine.Internationalization;

using MediatR;

namespace AppEngine.Authorization.UsersInPartition;

public class RolesQuery : IRequest<IEnumerable<RoleDescription>>;

public class RoleDescription
{
    public UserInPartitionRole Role { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}

public class RolesQueryHandler(Translator translator) : IRequestHandler<RolesQuery, IEnumerable<RoleDescription>>
{
    public Task<IEnumerable<RoleDescription>> Handle(RolesQuery query, CancellationToken cancellationToken)
    {
        var roles = new[] { UserInPartitionRole.Reader, UserInPartitionRole.Writer, UserInPartitionRole.Admin }
            .Select(rol => new RoleDescription
                           {
                               Role = rol,
                               Name = translator.TranslateEnum(rol),
                               Description = GetDescription(rol)
                           });

        return Task.FromResult(roles);
    }

    private string GetDescription(UserInPartitionRole role)
    {
        return translator.GetResourceString($"{nameof(UserInPartitionRole)}_{role}_Description");
    }
}