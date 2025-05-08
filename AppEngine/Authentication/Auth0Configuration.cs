using AppEngine.Configurations;

namespace AppEngine.Authentication;

public class Auth0Configuration : IConfigurationItem
{
    public string ClientIdKey { get; set; } = null!;
    public string ClientSecretKey { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string TokenUrl { get; set; } = null!;
    public string? UserEndpoint { get; set; }
}

public class DefaultAuth0Configuration : Auth0Configuration, IDefaultConfigurationItem
{
    public DefaultAuth0Configuration()
    {
        ClientIdKey = "Auth0-ClientId-ClubEngine";
        ClientSecretKey = "Auth0-ClientSecret-ClubEngine";
        Audience = "https://clubengine.eu.auth0.com/api/v2/";
        TokenUrl = "https://clubengine.eu.auth0.com/oauth/token";
        UserEndpoint = "https://clubengine.eu.auth0.com/api/v2/users";
    }
}