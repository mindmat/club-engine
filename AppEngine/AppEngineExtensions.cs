using System.Reflection;

using AppEngine.DataAccess;
using AppEngine.DependencyInjection;
using AppEngine.DomainEvents;
using AppEngine.ErrorHandling;
using AppEngine.Internationalization;
using AppEngine.Mediator;
using AppEngine.Partitions;
using AppEngine.Types;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppEngine;

public static class AppEngineExtensions
{
    public static void AddAppEngine(this WebApplicationBuilder builder, Assembly[] appAssemblies, string? databaseConnectionStringKey = null)
    {
        var assemblyContainer = new AppAssemblies([typeof(AppEngineExtensions).Assembly,.. appAssemblies]);
        builder.Services.AddSingleton(assemblyContainer);
        builder.Services.AddDbContext<DbContext, AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString(databaseConnectionStringKey ?? "database"),
                                                                                               b => b.MigrationsAssembly("ClubEngine.ApiService")));

        builder.Services.AddScoped(typeof(IQueryable<>), typeof(Queryable<>));
        builder.Services.AddScoped(typeof(Repository<>));

        builder.Services.AddMediatR(cfg =>
        {
            cfg.Lifetime = ServiceLifetime.Scoped;

            cfg.RegisterServicesFromAssemblies(assemblyContainer.Assemblies);
            //cfg.RegisterServicesFromAssemblies(assemblies);
        });

        builder.Services.AddSingleton(new RequestRegistry(assemblyContainer.Assemblies.GetTypesImplementing(typeof(IRequestHandler<,>)),
                                                          assemblyContainer.Assemblies.GetTypesImplementing(typeof(IRequestHandler<>))));
        builder.Services.AddSingleton<IApiDescriptionGroupCollectionProvider, MediatorEndpointApiDescriptionGroupCollectionProvider>();
        //builder.Services.AddOpenApiDocument(document => document.DocumentName = "v1");

        builder.Services.AddSingleton<TranslationAggregator>();

        //builder.Services.AddScoped<IHttpContextAccessor, HttpContextContainer>();

        builder.Services.AddScoped<ExceptionMiddleware>();
        builder.Services.AddSingleton<ExceptionTranslator>();
        //assemblies.GetTypesImplementing(typeof(IExceptionTranslation));
        builder.Services.AddTypesImplementingAsSingleton<IExceptionTranslation>(assemblyContainer.Assemblies);

        var partitionType = assemblyContainer.Assemblies.GetTypesImplementing(typeof(IPartition)).First();
        var partitionQueryableType = typeof(PartitionQueryable<>).MakeGenericType(partitionType);
        builder.Services.AddScoped(typeof(IQueryable<IPartition>), partitionQueryableType);
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