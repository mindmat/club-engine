using System.Diagnostics;

using AppEngine.Authorization;
using AppEngine.DataAccess;

namespace ClubEngine.ApiService.Members;

public class MembersQuery : IRequest<IEnumerable<MemberDisplayItem>>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public Guid?[]? MembershipTypeIds { get; set; }
    public string? SearchString { get; set; }
}

[DebuggerDisplay("{FirstName,nq} {LastName,nq} - {Email,nq}")]
public record MemberDisplayItem(Guid Id, string? FirstName, string? LastName, string? Email, Guid? CurrentMembershipTypeId);

public class MembersQueryHandler(IQueryable<Member> members) : IRequestHandler<MembersQuery, IEnumerable<MemberDisplayItem>>
{
    public async Task<IEnumerable<MemberDisplayItem>> Handle(MembersQuery query, CancellationToken cancellationToken)
    {
        return await members.Where(mbr => mbr.ClubId == query.PartitionId)
                            .WhereIf(query.MembershipTypeIds == null,
                                     mbr => mbr.CurrentMembershipTypeId_ReadModel != null)
                            .WhereIf(query.MembershipTypeIds?.Length > 0,
                                     mbr => query.MembershipTypeIds!.Contains(mbr.CurrentMembershipTypeId_ReadModel))
                            .WhereIf(!string.IsNullOrEmpty(query.SearchString),
                                     mbr => EF.Functions.Like(mbr.FirstName, $"%{query.SearchString}%")
                                         || EF.Functions.Like(mbr.LastName, $"%{query.SearchString}%")
                                         || EF.Functions.Like(mbr.Email, $"%{query.SearchString}%"))
                            .OrderBy(mbr => mbr.FirstName)
                            .ThenBy(mbr => mbr.LastName)
                            .Select(mbr => new MemberDisplayItem(mbr.Id, mbr.FirstName, mbr.LastName, mbr.Email, mbr.CurrentMembershipTypeId_ReadModel))
                            .ToListAsync(cancellationToken);
    }
}