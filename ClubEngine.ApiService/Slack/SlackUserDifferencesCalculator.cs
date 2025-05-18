using System.Diagnostics;

using AppEngine.Helpers;
using AppEngine.MenuNodes;
using AppEngine.ReadModels;
using AppEngine.Slack;

using ClubEngine.ApiService.Members;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.Slack;

public class SlackMatchingNodeKey : IMenuNodeKey;

public record SlackDifferences(IEnumerable<MemberDto> OnlyMember,
                               IEnumerable<SlackUser> OnlySlack,
                               IEnumerable<SlackMatch> Matches);

public record SlackMatch(MemberDto Member, SlackUser Slack);

[DebuggerDisplay("{FirstName,nq} {LastName,nq} - {Email,nq}")]
public record MemberDto(Guid Id, string? Email, string? FirstName, string? LastName, Guid? CurrentMembershipTypeId);

public class SlackUserDifferencesCalculator(SlackClient client, IQueryable<Member> members)
    : ReadModelCalculator<SlackDifferences>
{
    public override string QueryName => nameof(SlackUserDifferencesQuery);
    public override bool IsDateDependent => false;

    protected override async Task<(SlackDifferences ReadModel, MenuNodeCalculation? MenuNode)> CalculateTyped(Guid partitionId, Guid? rowId, CancellationToken cancellationToken)
    {
        var slackUsers = await client.GetAllMembers();

        var activeMembers = await members.Where(mbr => mbr.ClubId == partitionId
                                                    && mbr.CurrentMembershipTypeId_ReadModel != null)
                                         .Select(usr => new MemberDto(usr.Id,
                                                                      usr.Email,
                                                                      usr.FirstName,
                                                                      usr.LastName,
                                                                      usr.CurrentMembershipTypeId_ReadModel))
                                         .ToListAsync(cancellationToken);

        var compare = ListComparer.Compare(activeMembers, slackUsers, HasSameEmail);
        //var (extraMembers2, extraSlack2) = ListComparer.Compare(extraMembers, extraSlack, HasSameFirstName);

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

    private static bool HasSameFirstName(MemberDto member, SlackUser slackUser)
    {
        return member.FirstName != null
            && member.FirstName.Equals(slackUser.FirstName, StringComparison.InvariantCultureIgnoreCase);
    }

    private static bool HasSameEmail(MemberDto member, SlackUser slackUser)
    {
        return member.Email != null
            && member.Email.Equals(slackUser.Email, StringComparison.InvariantCultureIgnoreCase);
    }
}