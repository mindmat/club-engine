using System.Globalization;
using System.Reflection;
using System.Resources;

namespace AppEngine.Internationalization;

public class TranslationAggregator
{
    private readonly IDictionary<string, ResourceManager> _namespaceToResources;

    public TranslationAggregator(IEnumerable<ResourceManager> resourceManagers)
    {
        var baseNameProperty = typeof(ResourceManager).GetRuntimeProperty("BaseName");
        if (baseNameProperty == null)
        {
            throw new ArgumentNullException("ResourceManager does not have a BaseName anymore");
        }

        _namespaceToResources = resourceManagers.Select(rem => new { BaseName = (string?)baseNameProperty.GetValue(rem), ResourceManager = rem })
                                                .Where(tmp => tmp.BaseName != null)
                                                .ToDictionary(tmp => tmp.BaseName!.Replace(".Resources", string.Empty)
                                                                        .Replace(".Properties", string.Empty),
                                                              tmp => tmp.ResourceManager);
    }

    public string? GetResourceString(Type type, CultureInfo? cultureInfo = null)
    {
        return GetResourceString(type, type.Name, cultureInfo);
    }

    public string? GetResourceString(Type? type, string key, CultureInfo? cultureInfo = null)
    {
        if (type == null)
        {
            return null;
        }

        var resourceManager = GetResourceManagersForType(type);

        var result = resourceManager.Select(rsm => rsm.GetString(key, cultureInfo))
                                    .FirstOrDefault(val => val != null);
        if (result == null)
        {
            // Fallback: search in all resources (also when namespace doesn't match)
            return _namespaceToResources.Values.Select(rsm => rsm.GetString(key, cultureInfo))
                                        .FirstOrDefault(val => val != null);
        }

        return result;
    }
    
    public string GetResourceString(string key, string? typeNamespace = null, CultureInfo? cultureInfo = null)
    {
        if (typeNamespace != null)
        {
            var resourceManager = GetResourceManagersForType(typeNamespace);

            return resourceManager.Select(rsm => rsm.GetString(key, cultureInfo))
                                  .FirstOrDefault(val => val != null)
                   ?? key;
        }

        return _namespaceToResources.Values
                                    .Select(rsm => rsm.GetString(key))
                                    .FirstOrDefault(str => str != null)
               ?? key;
    }

    private IEnumerable<ResourceManager> GetResourceManagersForType(Type type)
    {
        return type.Namespace == null
                   ? []
                   : GetResourceManagersForType(type.Namespace);
    }

    private IEnumerable<ResourceManager> GetResourceManagersForType(string typeNamespace)
    {
        var resourceManager = _namespaceToResources.Where(ntr => typeNamespace.StartsWith(ntr.Key))
                                                   .Select(ntr => ntr.Value)
                                                   .ToArray();
        if (!resourceManager.Any())
        {
            resourceManager = _namespaceToResources.Values.ToArray();
        }

        return resourceManager;
    }
}