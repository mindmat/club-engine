using AppEngine.Authorization;
using AppEngine.TimeHandling;

namespace ClubEngine.ApiService.Clubs;

public class PeriodsQuery : IRequest<IEnumerable<PeriodDisplayItem>>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
}

public record PeriodDisplayItem(Guid Id, string Duration);

public class PeriodsQueryHandler(IQueryable<Period> periods, DateFormatter dateFormatter)
    : IRequestHandler<PeriodsQuery, IEnumerable<PeriodDisplayItem>>
{
    public async Task<IEnumerable<PeriodDisplayItem>> Handle(PeriodsQuery query, CancellationToken cancellationToken)
    {
        return await periods.Where(per => per.ClubId == query.PartitionId)
                            .OrderByDescending(per => per.From)
                            .Select(per => new PeriodDisplayItem(per.Id, per.Name ?? dateFormatter.GetPeriodText(per)))
                            .ToListAsync(cancellationToken);
    }
}