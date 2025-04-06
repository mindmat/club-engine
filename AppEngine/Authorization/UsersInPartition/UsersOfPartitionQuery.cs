using AppEngine.Internationalization;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Authorization.UsersInPartition;

public class UsersOfPartitionQuery : IRequest<IEnumerable<UserInPartitionDisplayItem>>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
}

public class UserInPartitionDisplayItem
{
    public Guid PartitionId { get; set; }
    public UserInPartitionRole Role { get; set; }
    public string RoleText { get; set; } = null!;
    public string? UserEmail { get; set; }
    public Guid UserId { get; set; }
    public string UserDisplayName { get; set; } = null!;
    public string? UserAvatarUrl { get; set; }
}

public class UsersOfPartitionQueryHandler(IQueryable<UserInPartition> usersInPartitions,
                                          Translator translator)
    : IRequestHandler<UsersOfPartitionQuery, IEnumerable<UserInPartitionDisplayItem>>
{
    public async Task<IEnumerable<UserInPartitionDisplayItem>> Handle(UsersOfPartitionQuery query,
                                                                      CancellationToken cancellationToken)
    {
        return await usersInPartitions.Where(uie => uie.PartitionId == query.PartitionId)
                                      .Select(uie => new UserInPartitionDisplayItem
                                      {
                                          PartitionId = uie.PartitionId,
                                          UserId = uie.UserId,
                                          Role = uie.Role,
                                          RoleText = translator.TranslateEnum(uie.Role),
                                          UserDisplayName = $"{uie.User!.FirstName} {uie.User.LastName}",
                                          UserEmail = uie.User.Email,
                                          UserAvatarUrl = uie.User.AvatarUrl
                                      })
                                      .ToListAsync(cancellationToken);
    }
}