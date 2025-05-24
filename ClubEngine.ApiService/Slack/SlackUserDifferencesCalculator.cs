using AppEngine.Helpers;
using AppEngine.MenuNodes;
using AppEngine.ReadModels;
using AppEngine.Slack;

using ClubEngine.ApiService.Members;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.Slack;

public class SlackMatchingNodeKey : IMenuNodeKey;

public record SlackDifferences(IEnumerable<MemberDisplayItem> OnlyMember,
                               IEnumerable<SlackUser> OnlySlack,
                               IEnumerable<SlackMatch> Matches);

public record SlackMatch(MemberDisplayItem Member, SlackUser Slack);

public class SlackUserDifferencesCalculator(SlackClient client,
                                            IQueryable<Member> members,
                                            IQueryable<SlackUserMapping> slackMappings)
    : ReadModelCalculator<SlackDifferences>
{
    public override string QueryName => nameof(SlackUserDifferencesQuery);
    public override bool IsDateDependent => false;

    protected override async Task<(SlackDifferences ReadModel, MenuNodeCalculation? MenuNode)> CalculateTyped(Guid partitionId, Guid? rowId, CancellationToken cancellationToken)
    {
        var slackUsers = await client.GetAllMembers();

        var mappings = await slackMappings.Where(sum => sum.Member!.ClubId == partitionId)
                                          .ToListAsync(cancellationToken);

        var activeMembers = await members.Where(mbr => mbr.ClubId == partitionId
                                                    && mbr.CurrentMembershipTypeId_ReadModel != null)
                                         .Select(usr => new MemberDisplayItem(usr.Id,
                                                                              usr.FirstName,
                                                                              usr.LastName,
                                                                              usr.Email,
                                                                              usr.CurrentMembershipTypeId_ReadModel))
                                         .ToListAsync(cancellationToken);

        var compare = ListComparer.Compare(activeMembers,
                                           slackUsers,
                                           (item, user) => IsManuallyMapped(item, user, mappings),
                                           HasSameEmail);

        var node = new MenuNodeCalculation
                   {
                       Key = nameof(SlackMatchingNodeKey),
                       Style = MenuNodeStyle.Info,
                       Content = $"{compare.OnlyLeft.Count().ToString()}"
                   };

        return (new SlackDifferences(compare.OnlyLeft,
                                     compare.OnlyRight,
                                     compare.Matches.Select(mat => new SlackMatch(mat.Item1, mat.Item2))),
            node);
    }

    private static bool IsManuallyMapped(MemberDisplayItem member, SlackUser slackUser, List<SlackUserMapping> mappings)
    {
        return mappings.Any(sum => sum.MemberId == member.Id
                                && sum.SlackUserId == slackUser.Id);
    }

    private static bool HasSameEmail(MemberDisplayItem member, SlackUser slackUser)
    {
        return member.Email != null
            && member.Email.Equals(slackUser.Email, StringComparison.InvariantCultureIgnoreCase);
    }
}