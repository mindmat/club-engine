using System.Reflection;

using AppEngine.Types;

using Microsoft.Extensions.DependencyInjection;

namespace AppEngine.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddTypesImplementingAsSingleton<TInterface>(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        var types = assemblies.GetTypesImplementing(typeof(TInterface));

        foreach (var type in types)
        {
            services.AddSingleton(typeof(TInterface), type);
        }
    }
    public static void AddTypesImplementingAsScoped<TInterface>(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        var types = assemblies.GetTypesImplementing(typeof(TInterface));

        foreach (var type in types)
        {
            services.AddScoped(typeof(TInterface), type);
        }
    }
}