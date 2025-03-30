using System.Reflection;

using AppEngine.DataAccess;
using AppEngine.DependencyInjection;
using AppEngine.DomainEvents;
using AppEngine.ErrorHandling;
using AppEngine.Internationalization;
using AppEngine.Mediator;
using AppEngine.Types;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AppEngine;

public static class AppEngineExtensions
{
    public static void AddAppEngine(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddScoped(typeof(IQueryable<>), typeof(Queryable<>));
        services.AddScoped(typeof(Repository<>));

        services.AddMediatR(cfg =>
        {
            cfg.Lifetime = ServiceLifetime.Scoped;

            cfg.RegisterServicesFromAssemblies(assemblies);
            //cfg.RegisterServicesFromAssemblies(assemblies);
        });
        services.AddSingleton(new RequestRegistry(assemblies.GetTypesImplementing(typeof(IRequestHandler<,>)),
                                                  assemblies.GetTypesImplementing(typeof(IRequestHandler<>))));
        services.AddSingleton<IApiDescriptionGroupCollectionProvider, MediatorEndpointApiDescriptionGroupCollectionProvider>();
        //services.AddOpenApiDocument(document => document.DocumentName = "v1");

        services.AddSingleton<TranslationAggregator>();

        //services.AddScoped<IHttpContextAccessor, HttpContextContainer>();

        services.AddScoped<ExceptionMiddleware>();
        services.AddSingleton<ExceptionTranslator>();
        //assemblies.GetTypesImplementing(typeof(IExceptionTranslation));
        services.AddTypesImplementingAsSingleton<IExceptionTranslation>(assemblies);
    }

    public static void MapAppEngineEndpoints(this IEndpointRouteBuilder endpoints, IServiceProvider services)
    {
        endpoints.MapRequests(services);
        endpoints.MapHub<NotificationHub>("/notifications");
    }

    public static void MapAppEngineEndpoints(this WebApplication app)
    {
        app.MapRequests(app.Services);
        app.MapHub<NotificationHub>("/notifications");
    }
}