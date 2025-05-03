using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.Clubs;

[AllowAnonymous]
public class ClubsQuery : IRequest<IEnumerable<ClubListItem>>;

public class ClubsQueryHandler(IQueryable<Club> clubs) : IRequestHandler<ClubsQuery, IEnumerable<ClubListItem>>
{
    public async Task<IEnumerable<ClubListItem>> Handle(ClubsQuery request, CancellationToken cancellationToken)
    {
        return await clubs.Select(clb => new ClubListItem(clb.Id, clb.Name, clb.Acronym))
                          .ToListAsync(cancellationToken);
    }
}

public record ClubListItem(Guid Id, string Name, string Acronym);