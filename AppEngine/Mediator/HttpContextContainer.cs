using Microsoft.AspNetCore.Http;

namespace AppEngine.Mediator;

public class HttpContextContainer : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }
}