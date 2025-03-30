using System.Reflection;
using AppEngine.Internationalization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AppEngine.Mediator;

public class MediatorEndpointApiDescriptionGroupCollectionProvider(RequestRegistry requestRegistry,
                                                                   TranslationAggregator translationAggregator) 
    : IApiDescriptionGroupCollectionProvider
{
    public int Order => 1;

    public ApiDescriptionGroupCollection ApiDescriptionGroups
    {
        get
        {
            var apis = new List<ApiDescription>();
            foreach (var requestType in requestRegistry.RequestTypes)
            {
                var (request, suffix) = Split(requestType.Request.Name);
                var controllerActionDescriptor = new ControllerActionDescriptor
                                                 {
                                                     DisplayName = translationAggregator.GetResourceString(requestType.Request.Name),
                                                     ControllerName = request,
                                                     ActionName = suffix,
                                                     ControllerTypeInfo = requestType.RequestHandler.GetTypeInfo(),
                                                     MethodInfo = requestType.RequestHandler.GetMethod("Handle")!,
                                                     Parameters = new List<ParameterDescriptor>
                                                                  {
                                                                      new()
                                                                      {
                                                                          Name = requestType.Request.Name,
                                                                          ParameterType = requestType.Request
                                                                      }
                                                                  }
                                                 };

                var apiDescription = new ApiDescription
                                     {
                                         GroupName = "Mediator",
                                         HttpMethod = "Post",
                                         RelativePath = "/api/" + requestType.Request.Name,
                                         ActionDescriptor = controllerActionDescriptor,
                                         ParameterDescriptions =
                                         {
                                             new ApiParameterDescription
                                             {
                                                 Name = requestType.Request.Name,
                                                 Type = requestType.Request,
                                                 Source = BindingSource.Body
                                             }
                                         },
                                         SupportedRequestFormats = { new ApiRequestFormat { MediaType = "application/json" } }
                                     };

                if (requestType.Type == RequestType.Query)
                {
                    var responseTypes = new ApiResponseTypeProvider().GetApiResponseTypes(apiDescription, requestType.Request);

                    foreach (var responseType in responseTypes)
                    {
                        apiDescription.SupportedResponseTypes.Add(responseType);
                    }
                }
                else
                {
                    apiDescription.SupportedResponseTypes.Add(new ApiResponseType { StatusCode = 204 });
                }

                apis.Add(apiDescription);
            }

            var group = new ApiDescriptionGroup("Mediator", apis);

            return new ApiDescriptionGroupCollection([group], 1);
        }
    }

    /// <summary>
    /// RegistrablesOverviewQuery -> (RegistrablesOverview, Query)
    /// </summary>
    /// <param name="requestName"></param>
    /// <returns></returns>
    private static (string Request, string Suffix) Split(string requestName)
    {
        var commandIndex = requestName.LastIndexOf("Command", StringComparison.InvariantCulture);
        if (commandIndex > 0)
        {
            return (requestName[..commandIndex], requestName[commandIndex..]);
        }

        var queryIndex = requestName.LastIndexOf("Query", StringComparison.InvariantCulture);
        if (queryIndex > 0)
        {
            return (requestName[..queryIndex], requestName[queryIndex..]);
        }

        return (requestName, string.Empty);
    }
}