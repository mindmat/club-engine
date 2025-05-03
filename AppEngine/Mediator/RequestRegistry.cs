using AppEngine.Types;

using MediatR;

using Microsoft.AspNetCore.Authorization;

namespace AppEngine.Mediator;

public record RequestMetadata(Type Request, Type RequestHandler, RequestType Type, RequestKind Kind, string Url);

public enum RequestKind
{
    Default   = 0,
    Upload    = 1,
    PublicApi = 2
}

public class RequestRegistry
{
    public IEnumerable<RequestMetadata> RequestTypes { get; }

    public RequestRegistry(IEnumerable<Type> requestQueryTypes, IEnumerable<Type> requestCommandTypes)
    {
        RequestTypes = Enumerable.Concat(requestQueryTypes.Select(rht =>
                                         {
                                             var queryType = rht.GetInterface(typeof(IRequestHandler<,>).Name)!.GetGenericArguments()[0];
                                             var kind = GetKind(queryType);

                                             return new RequestMetadata(queryType,
                                                                        rht,
                                                                        RequestType.Query,
                                                                        kind,
                                                                        GetUrl(queryType, kind));
                                         }),
                                         requestCommandTypes.Select(rht =>
                                         {
                                             var commandType = rht.GetInterface(typeof(IRequestHandler<>).Name)!.GetGenericArguments()[0];
                                             var kind = GetKind(commandType);

                                             return new RequestMetadata(commandType,
                                                                        rht,
                                                                        RequestType.Command,
                                                                        kind,
                                                                        GetUrl(commandType, kind));
                                         }))
                                 .OrderBy(rht => rht.Request.Name)
                                 .ToList();
    }

    private RequestKind GetKind(Type requestType)
    {
        if (requestType.ImplementsInterface<IReceiveFileCommand>())
        {
            return RequestKind.Upload;
        }

        if (requestType.HasAttribute<AllowAnonymousAttribute>())
        {
            return RequestKind.PublicApi;
        }

        return RequestKind.Default;
    }

    public string GetUrl(Type requestType, RequestKind kind)
    {
        return kind switch
        {
            RequestKind.Default   => $"/api/{requestType.Name}",
            RequestKind.PublicApi => $"/public-api/{requestType.Name}",
            RequestKind.Upload    => "/api/{partition}/upload/" + requestType.Name,
            _                     => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }
}

public enum RequestType
{
    Query   = 1,
    Command = 2
}