using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AppEngine.Slack;

public class SlackClient(SlackTokenProvider tokenProvider,
                         SlackConfiguration configuration,
                         HttpClient httpClient)
{
    public async Task<IEnumerable<SlackUser>> GetAllMembers()
    {
        var token = await tokenProvider.GetToken();
        var url = configuration.BaseUrl + "users.list";

        var request = new HttpRequestMessage(HttpMethod.Get, url)
                      {
                          Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
                      };
        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Slack API error: {response.ReasonPhrase} ({response.StatusCode})");
        }

        var options = new JsonSerializerOptions
                      {
                          PropertyNameCaseInsensitive = true,
                          IncludeFields = true
                      };
        //var users = JsonSerializer.Deserialize<UserListResponse>(json, options);
        var slackUsers = await response.Content.ReadFromJsonAsync<UserListResponse>(options);

        if (slackUsers?.ok != true || slackUsers?.members == null)
        {
            throw new Exception($"Slack API error: {slackUsers?.error}");
        }

        return slackUsers.members
                         .Where(usr => usr is { is_bot: false, deleted: false })
                         .Select(usr => new SlackUser(usr.id,
                                                      usr.profile.email,
                                                      usr.profile.first_name,
                                                      usr.profile.last_name,
                                                      usr.is_invited_user));
    }

    public record UserListResponse : Response
    {
        public bool   ok;
        public User[] members;
    }

    public record User
    {
        public string id;

        public bool IsSlackBot
        {
            get { return id.Equals("USLACKBOT", StringComparison.CurrentCultureIgnoreCase); }
        }

        public string      name;
        public bool        deleted;
        public string      color;
        public UserProfile profile;
        public bool        is_admin;
        public bool        is_owner;
        public bool        is_primary_owner;
        public bool        is_restricted;
        public bool        is_ultra_restricted;
        public bool        is_invited_user;
        public bool        has_2fa;
        public string      two_factor_type;
        public bool        has_files;
        public string      presence;
        public bool        is_bot;
        public string      tz;
        public string      tz_label;
        public int         tz_offset;
        public string      team_id;
        public string      real_name;
    }

    public abstract record Response
    {
        /// <summary>
        /// Should always be checked before trying to process a response.
        /// </summary>
        public bool ok;

        /// <summary>
        /// if ok is false, then this is the reason-code
        /// </summary>
        public string error;

        public string needed;
        public string provided;
        public string warning;

        public ResponseMetaData response_metadata;
    }

    public record ResponseMetaData
    {
        public string   next_cursor;
        public string[] messages;
    }

    public record AccessTokenResponse : Response
    {
        public string           access_token;
        public string           scope;
        public string           team_name;
        public string           team_id;
        public BotTokenResponse bot;
        public IncomingWebhook  incoming_webhook;
    }

    public record BotTokenResponse
    {
        public string emoji;
        public string image_24;
        public string image_32;
        public string image_48;
        public string image_72;
        public string image_192;

        public bool        deleted;
        public UserProfile icons;
        public string      id;
        public string      name;
        public string      bot_user_id;
        public string      bot_access_token;
    }

    public record IncomingWebhook
    {
        public string channel;
        public string channel_id;
        public string configuration_url;
        public string url;
    }

    [DebuggerDisplay("{real_name,nq}")]
    public record UserProfile : ProfileIcons
    {
        public string title;
        public string display_name;
        public string first_name;
        public string last_name;
        public string real_name;
        public string email;
        public string skype;
        public string status_emoji;
        public string status_text;
        public string phone;
    }

    public record ProfileIcons
    {
        public string image_24;
        public string image_32;
        public string image_48;
        public string image_72;
        public string image_192;
        public string image_512;
    }
}

public record SlackUser(string Id, string Email, string FirstName, string LastName, bool IsInvitedUser);