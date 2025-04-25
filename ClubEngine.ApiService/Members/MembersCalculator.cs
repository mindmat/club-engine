using AppEngine.MenuNodes;
using AppEngine.ReadModels;
using AppEngine.TimeHandling;
using AppEngine.Types;

using ClubEngine.ApiService.Members.Memberships;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.Members;

public class MembersNodeKey : IMenuNodeKey;

public class MembersCalculator(IQueryable<Member> members,
                               RequestTimeProvider timeProvider)
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

        var stats = datesWithChanges.SelectMany(date => allMembers.Select(mbr => mbr.Memberships!.FirstOrDefault(msp => msp.IsActiveAt(date))?.MembershipTypeId)
                                                                  .WhereNotNull()
                                                                  .GroupBy(mst => mst)
                                                                  .Select(grp => new { MembershipTypeId = grp.Key, Date = date, Count = grp.Count() }))
                                    .ToList();

        var currentTotal = stats.Where(stat => stat.Date == timeProvider.RequestToday)
                                .Sum(c => c.Count);

        var readModel = new MemberStats(currentTotal,
                                        stats.GroupBy(stt => stt.MembershipTypeId)
                                             .Select(grp => new MemberCount(grp.Key, grp.Select(stt => new MembershipTypeCount(stt.Date, stt.Count)))));

        return (readModel, node);
    }
}

public record MemberStats(int CurrentTotal, IEnumerable<MemberCount> MemberCounts);

public record MemberCount(Guid MembershipTypeId, IEnumerable<MembershipTypeCount> Counts);

public record MembershipTypeCount(DateOnly Date, int Count);