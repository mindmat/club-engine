using System.Diagnostics;

using AppEngine.Secrets;

namespace AppEngine.Slack;

public class SlackTokenProvider(SlackConfiguration config,
                                SecretReader secretReader)
{
    public async Task<string?> GetToken()
    {
        return await secretReader.GetSecret(config.CodeSecretKey);
    }
}