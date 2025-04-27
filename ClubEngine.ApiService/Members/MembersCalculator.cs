using AppEngine.MenuNodes;
using AppEngine.ReadModels;
using AppEngine.TimeHandling;
using AppEngine.Types;

using ClubEngine.ApiService.Members.Memberships;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.Members;

public class MembersNodeKey : IMenuNodeKey;

public class MembersCalculator(IQueryable<Member> members,
                               IQueryable<MembershipType> membershipTypes,
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

        var types = await membershipTypes.Where(mst => mst.ClubId == partitionId)
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

        var currentCounts = stats.Where(stat => stat.Date == timeProvider.RequestToday)
                                 .ToList();

        var readModel = new MemberStats(currentCounts.Sum(c => c.Count),
                                        currentCounts.Select(c =>
                                        {
                                            var type = types.Find(typ => typ.Id == c.MembershipTypeId);

                                            return new MemberCurrentCount(c.MembershipTypeId,
                                                                          c.Count,
                                                                          type?.FallbackName ?? "?",
                                                                          type?.ShowInOverview ?? false);
                                        }),
                                        stats.GroupBy(stt => stt.MembershipTypeId)
                                             .Select(grp =>
                                             {
                                                 var type = types.Find(typ => typ.Id == grp.Key);

                                                 return new MemberCount(grp.Key,
                                                                        type?.FallbackName ?? "?",
                                                                        grp.Select(stt => new MembershipTypeCount(stt.Date, stt.Count)));
                                             }));

        return (readModel, node);
    }
}

public record MemberStats(int CurrentTotal, IEnumerable<MemberCurrentCount> CurrentCounts, IEnumerable<MemberCount> MemberCounts);

public record MemberCount(Guid MembershipTypeId, string Name, IEnumerable<MembershipTypeCount> Counts);

public record MemberCurrentCount(Guid MembershipTypeId, int Count, string Name, bool ShowInOverview);

public record MembershipTypeCount(DateOnly Date, int Count);