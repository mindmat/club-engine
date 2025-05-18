using AppEngine.Authorization;
using AppEngine.ReadModels;

using MediatR;

namespace ClubEngine.ApiService.Slack;

public class SlackUserDifferencesQuery : IRequest<SlackDifferences>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public string? SearchString { get; set; }
}

public class SlackUserDifferencesQueryHandler(ReadModelReader readModelReader) : IRequestHandler<SlackUserDifferencesQuery, SlackDifferences>
{
    public async Task<SlackDifferences> Handle(SlackUserDifferencesQuery query, CancellationToken cancellationToken)
    {
        var differences = await readModelReader.GetDeserialized<SlackDifferences>(nameof(SlackUserDifferencesQuery),
                                                                                  query.PartitionId,
                                                                                  (Guid?)null,
                                                                                  cancellationToken);

        if (string.IsNullOrWhiteSpace(query.SearchString))
        {
            return differences;
        }

        var splits = query.SearchString.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var membersFiltered = differences.OnlyMember;
        var slackFiltered = differences.OnlySlack;
        var matchedFiltered = differences.Matches;

        foreach (var split in splits)
        {
            membersFiltered = membersFiltered.Where(mbr => mbr.FirstName?.Contains(split, StringComparison.InvariantCultureIgnoreCase) == true
                                                        || mbr.LastName?.Contains(split, StringComparison.InvariantCultureIgnoreCase) == true
                                                        || mbr.Email?.Contains(split, StringComparison.InvariantCultureIgnoreCase) == true);

            slackFiltered = slackFiltered.Where(usr => usr.FirstName?.Contains(split, StringComparison.InvariantCultureIgnoreCase) == true
                                                    || usr.LastName?.Contains(split, StringComparison.InvariantCultureIgnoreCase) == true
                                                    || usr.Email?.Contains(split, StringComparison.InvariantCultureIgnoreCase) == true);

            matchedFiltered = matchedFiltered.Where(m => m.Member.FirstName?.Contains(split, StringComparison.InvariantCultureIgnoreCase) == true
                                                      || m.Member.LastName?.Contains(split, StringComparison.InvariantCultureIgnoreCase) == true
                                                      || m.Member.Email?.Contains(split, StringComparison.InvariantCultureIgnoreCase) == true
                                                      || m.Slack.FirstName?.Contains(split, StringComparison.InvariantCultureIgnoreCase) == true
                                                      || m.Slack.LastName?.Contains(split, StringComparison.InvariantCultureIgnoreCase) == true
                                                      || m.Slack.Email?.Contains(split, StringComparison.InvariantCultureIgnoreCase) == true);
        }

        return new SlackDifferences(membersFiltered,
                                    slackFiltered,
                                    matchedFiltered);
    }
}