using System.Reflection;

using AppEngine.Authentication;
using AppEngine.Authorization;
using AppEngine.Authorization.UsersInPartition;
using AppEngine.Configurations;
using AppEngine.DataAccess;
using AppEngine.DependencyInjection;
using AppEngine.DomainEvents;
using AppEngine.ErrorHandling;
using AppEngine.Internationalization;
using AppEngine.Json;
using AppEngine.Mediator;
using AppEngine.Partitions;
using AppEngine.ReadModels;
using AppEngine.Secrets;
using AppEngine.ServiceBus;
using AppEngine.TimeHandling;
using AppEngine.Types;

using Azure.Identity;
using Azure.Messaging.ServiceBus;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppEngine;

public static class AppEngineExtensions
{
    public static void AddAppEngine(this WebApplicationBuilder builder, Assembly[] appAssemblies, string? databaseConnectionStringKey = null)
    {
        var assemblyContainer = new AppAssemblies([typeof(AppEngineExtensions).Assembly, .. appAssemblies]);
        builder.Services.AddSingleton(assemblyContainer);

        builder.Services.AddDbContext<DbContext, AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString(databaseConnectionStringKey ?? "database"),
                                                                                               b => b.MigrationsAssembly("ClubEngine.ApiService")));

        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddApplicationInsights()
                                                                    .SetMinimumLevel(LogLevel.Information));

        builder.Services.AddScoped(typeof(IQueryable<>), typeof(Queryable<>));
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        RegisterMediator(builder, assemblyContainer);
        RegisterIam(builder);

        //builder.Services.AddOpenApiDocument(document => document.DocumentName = "v1");

        builder.Services.AddSingleton<Translator>();

        var domainEventTypes = assemblyContainer.Assemblies.GetTypesImplementing(typeof(DomainEvent));
        builder.Services.AddSingleton(services => new DomainEventCatalog(domainEventTypes, services.GetService<Translator>()!));
        builder.Services.AddScoped<DomainEventCatalog>();

        //builder.Services.AddScoped<IHttpContextAccessor, HttpContextContainer>();

        builder.Services.AddScoped<ExceptionMiddleware>();
        builder.Services.AddSingleton<ExceptionTranslator>();
        //assemblies.GetTypesImplementing(typeof(IExceptionTranslation));
        builder.Services.AddTypesImplementingAsSingleton<IExceptionTranslation>(assemblyContainer.Assemblies);

        var partitionType = assemblyContainer.Assemblies.GetTypesImplementing(typeof(IPartition)).First();
        var partitionQueryableType = typeof(PartitionQueryable<>).MakeGenericType(partitionType);
        builder.Services.AddScoped(typeof(IQueryable<IPartition>), partitionQueryableType);
        builder.Services.AddScoped<PartitionContext>();
        builder.Services.AddScoped<PartitionAcronymResolver, PartitionAcronymResolver>();

        builder.Services.AddScoped<ChangeTrigger>();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddScoped<RequestTimeProvider>();

        builder.Services.AddSingleton<SecretReader>();

        builder.Services.AddSingleton<Serializer>();
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpContextAccessor();

        RegisterCommandQueue(builder);
        RegisterConfigurations(builder, assemblyContainer);
    }

    private static void RegisterConfigurations(WebApplicationBuilder builder, AppAssemblies assemblyContainer)
    {
        builder.Services.AddScoped<ConfigurationRegistry>();

        var defaultConfigItemTypes = assemblyContainer.Assemblies.GetTypesImplementing(typeof(IDefaultConfigurationItem))
                                                      .ToList();

        foreach (var defaultConfigItemType in defaultConfigItemTypes)
        {
            builder.Services.AddSingleton(typeof(IDefaultConfigurationItem), defaultConfigItemType);
        }

        var configTypes = assemblyContainer.Assemblies.GetTypesImplementing(typeof(IConfigurationItem))
                                           .Except(defaultConfigItemTypes);

        foreach (var configType in configTypes)
        {
            builder.Services.AddScoped(configType, services => services.GetService<ConfigurationRegistry>()!.GetConfigurationTypeless(configType));

            // add the possibility to inject FeatureConfigurations to singletons
            builder.Services.AddSingleton(typeof(SingletonConfigurationFeature<>).MakeGenericType(configType),
                                          services => GetSingletonConfig(services, configType));
        }
    }

    private static void RegisterCommandQueue(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<MessageQueueReceiver>();
        builder.Services.AddScoped<CommandQueue>();
        builder.Services.AddScoped<IEventBus, EventBus>();
        builder.Services.AddScoped<SourceQueueProvider>();
        //builder.Services.AddScoped(typeof(IEventToUserTranslation<>), assemblies);

        builder.Services.AddSingleton(_ =>
        {
            var cs = builder.Configuration.GetValue<string>("ServiceBus_ConnectionString");

            return cs != null
                ? new ServiceBusClient(cs)
                : new ServiceBusClient(builder.Configuration.GetValue<string>("ServiceBusNamespace"), new DefaultAzureCredential());
        });
        builder.Services.AddSingleton(services => services.GetService<ServiceBusClient>()!.CreateSender(CommandQueue.CommandQueueName));
    }

    private static void RegisterIam(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IIdentityProvider, Auth0IdentityProvider>();
        builder.Services.AddSingleton<Auth0TokenProvider>();

        builder.Services.AddScoped(services => new AuthenticatedUserId(services.GetService<IAuthenticatedUserProvider>()!
                                                                               .GetAuthenticatedUserId()
                                                                               .Result));

        builder.Services.AddScoped(services => services.GetService<IAuthenticatedUserProvider>()!
                                                       .GetAuthenticatedUser());

        builder.Services.AddScoped<IRightsOfPartitionRoleProvider, RightsOfPartitionRoleProvider>();
        builder.Services.AddScoped<IAuthorizationChecker, AuthorizationChecker>();
        builder.Services.AddScoped<IAuthenticatedUserProvider, AuthenticatedUserProvider>();
        builder.Services.AddScoped<RightsOfUserInPartitionCache>();
    }

    private static void RegisterMediator(WebApplicationBuilder builder, AppAssemblies assemblyContainer)
    {
        builder.Services.AddMediatR(cfg =>
        {
            cfg.Lifetime = ServiceLifetime.Scoped;
            
            cfg.RegisterServicesFromAssemblies(assemblyContainer.Assemblies);
            cfg.AddOpenBehavior(typeof(ExtractPartitionIdDecorator<,>));
            //cfg.AddOpenBehavior(typeof(AuditLogger<,>));
            cfg.AddOpenBehavior(typeof(CommitUnitOfWorkDecorator<,>));
        });

        builder.Services.AddSingleton(new RequestRegistry(assemblyContainer.Assemblies.GetTypesImplementing(typeof(IRequestHandler<,>)),
                                                          assemblyContainer.Assemblies.GetTypesImplementing(typeof(IRequestHandler<>))));
        builder.Services.AddSingleton<IApiDescriptionGroupCollectionProvider, MediatorEndpointApiDescriptionGroupCollectionProvider>();
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

    static object GetSingletonConfig(IServiceProvider services, Type featureConfigType)
    {
        using var scope = services.CreateScope();

        var singletonConfigurationFeatureType = typeof(SingletonConfigurationFeature<>).MakeGenericType(featureConfigType);
        var constructor = singletonConfigurationFeatureType.GetConstructor([featureConfigType]);

        return constructor?.Invoke([scope.ServiceProvider.GetService(featureConfigType)])!;
    }
}