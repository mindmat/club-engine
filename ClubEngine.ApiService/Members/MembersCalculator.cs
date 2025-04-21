using AppEngine.MenuNodes;
using AppEngine.ReadModels;
using AppEngine.TimeHandling;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.Members;

public class MembersNodeKey : IMenuNodeKey;

public class MembersCalculator(IQueryable<Member> members, RequestTimeProvider timeProvider)
    : ReadModelCalculator<MemberStats>
{
    public override string QueryName => nameof(MemberStatsQuery);
    public override bool IsDateDependent => true;

    protected override async Task<(MemberStats ReadModel, MenuNodeCalculation? MenuNode)> CalculateTyped(Guid partitionId, Guid? rowId, CancellationToken cancellationToken)
    {
        var allMembers = await members.Where(mbr => mbr.ClubId == partitionId)
                                      .Include(mbr => mbr.Memberships)
                                      .ToListAsync(cancellationToken);

        var node = new MenuNodeCalculation
                   {
                       Key = nameof(MembersNodeKey),
                       Style = MenuNodeStyle.Info,
                       Content = allMembers.Count.ToString()
                   };

        var datesWithChanges = Enumerable.Concat(allMembers.SelectMany(mbr => mbr.Memberships!.Select(msp => msp.From)),
                                                 allMembers.SelectMany(mbr => mbr.Memberships!.Select(msp => msp.Until)))
                                         .Append(timeProvider.RequestToday)
                                         .Where(date => date != DateOnly.MinValue
                                                     && date != DateOnly.MaxValue)
                                         .Distinct()
                                         .OrderBy(date => date);

        var stats = datesWithChanges.Select(date => new MemberCount(date, allMembers.Count(mbr => mbr.Memberships!.Any(msp => msp.IsActiveAt(date)))))
                                    .ToList();
        var currentTotal = stats.Single(stat => stat.Date == timeProvider.RequestToday).Total;
        var readModel = new MemberStats(currentTotal, stats);

        return (readModel, node);
    }
}

public record MemberStats(int CurrentTotal, IEnumerable<MemberCount> MemberCounts);

public record MemberCount(DateOnly Date, int Total);