using AppEngine.Helpers;
using AppEngine.Slack;

using ClubEngine.ApiService.Members;

namespace ClubEngine.Tests;

public class ListComparerTests
{
    [Fact]
    public void Test()
    {
        Member[] members =
        [
            new Member
            {
                Id = Guid.NewGuid(),
                FirstName = "Klara",
                LastName = "Meier",
                Email = "klara.meier@mail.com"
            },
        ];

        SlackUser[] slackUsers =
        [
            new("123", "klara@meier.com", "Klara", "M", false)
        ];
        var compare = ListComparer.Compare(members, slackUsers, (m, s) => m.Email == s.Email);
    }
}