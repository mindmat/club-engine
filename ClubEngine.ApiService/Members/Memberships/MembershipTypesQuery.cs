using AppEngine.Authorization;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.Members.Memberships;

public class MembershipTypesQuery : IRequest<IEnumerable<MembershipTypeItem>>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
}

public record MembershipTypeItem(Guid Id, string Name, string? Color);

public class MembershipTypesQueryHandler(IQueryable<MembershipType> membershipTypes) : IRequestHandler<MembershipTypesQuery, IEnumerable<MembershipTypeItem>>
{
    public async Task<IEnumerable<MembershipTypeItem>> Handle(MembershipTypesQuery request, CancellationToken cancellationToken)
    {
        return await membershipTypes.Where(mst => mst.ClubId == request.PartitionId)
                                    .Select(mst => new MembershipTypeItem(mst.Id, mst.FallbackName, mst.Color))
                                    .ToListAsync(cancellationToken);
    }
}