using System.IdentityModel.Tokens.Jwt;

namespace AppEngine.Authentication;

public static class JwtSecurityTokenExtensionMethods
{
    public static string GetClaim(this JwtSecurityToken token, string claimKey)
    {
        return token.Claims.FirstOrDefault(clm => clm.Type == claimKey)?.Value;
    }
}