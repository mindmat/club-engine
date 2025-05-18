using AppEngine.Configurations;

namespace AppEngine.Slack;

public class SlackConfiguration : IConfigurationItem
{
    public string ClientIdKey { get; set; } = null!;
    public string ClientSecretKey { get; set; } = null!;
    public string CodeSecretKey { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
}

public class DefaultSlackConfiguration : SlackConfiguration, IDefaultConfigurationItem
{
    public DefaultSlackConfiguration()
    {
        ClientIdKey = "Slack-ClientId";
        ClientSecretKey = "Slack-ClientSecret";
        CodeSecretKey = "Slack-Code";

        BaseUrl = "https://slack.com/api/";
    }
}