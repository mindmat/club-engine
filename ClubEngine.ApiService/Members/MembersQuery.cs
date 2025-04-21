using AppEngine.Authorization;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.Members;

public class MembersQuery : IRequest<IEnumerable<MemberDisplayItem>>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
}

public record MemberDisplayItem(Guid Id, string Name, string? Email, Guid? MembershipTypeId);

public class MembersQueryHandler(IQueryable<Member> members) : IRequestHandler<MembersQuery, IEnumerable<MemberDisplayItem>>
{
    public async Task<IEnumerable<MemberDisplayItem>> Handle(MembersQuery query, CancellationToken cancellationToken)
    {
        return await members.Where(mbr => mbr.ClubId == query.PartitionId)
                            .Select(mbr => new MemberDisplayItem(mbr.Id, $"{mbr.FirstName} {mbr.LastName}", mbr.Email, mbr.CurrentMembershipTypeId_ReadModel))
                            .ToListAsync(cancellationToken);
    }
}