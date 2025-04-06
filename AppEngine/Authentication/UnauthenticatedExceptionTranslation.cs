using System.Net;

using AppEngine.ErrorHandling;

namespace AppEngine.Authentication;

public class UnauthenticatedExceptionTranslation : ExceptionTranslation<UnauthorizedAccessException>
{
    public UnauthenticatedExceptionTranslation()
    {
        HttpCode = HttpStatusCode.Unauthorized;
    }

    public override object Translate(UnauthorizedAccessException ex)
    {
        return ex.Message;
    }
}