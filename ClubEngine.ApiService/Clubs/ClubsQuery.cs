using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.Clubs;

public class ClubsQuery : IRequest<IEnumerable<ClubListItem>>;

public class ClubsQueryHandler(IQueryable<Club> clubs) : IRequestHandler<ClubsQuery, IEnumerable<ClubListItem>>
{
    public async Task<IEnumerable<ClubListItem>> Handle(ClubsQuery request, CancellationToken cancellationToken)
    {
        return await clubs.Select(clb => new ClubListItem(clb.Id, clb.Name))
                          .ToListAsync(cancellationToken);
    }
}

public record ClubListItem(Guid Id, string Name);