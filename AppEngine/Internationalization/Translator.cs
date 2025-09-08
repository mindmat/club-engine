using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace AppEngine.Internationalization;

public class Translator
{
    private readonly IDictionary<string, ResourceManager> _namespaceToResources;

    public Translator(IEnumerable<ResourceManager> resourceManagers)
    {
        var baseNameProperty = typeof(ResourceManager).GetRuntimeProperty("BaseName");

        if (baseNameProperty == null)
        {
            throw new ArgumentNullException("BaseName property of ResourceManager not found");
        }

        _namespaceToResources = resourceManagers.Select(rem => new { BaseName = (string?)baseNameProperty.GetValue(rem), ResourceManager = rem })
                                                .Where(tmp => tmp.BaseName != null)
                                                .ToDictionary(tmp => tmp.BaseName!.Replace(".Resources", string.Empty)
                                                                        .Replace(".Properties", string.Empty),
                                                              tmp => tmp.ResourceManager);
    }

    public string TranslateEnum<TEnum>(TEnum value)
        where TEnum : struct
    {
        return GetResourceString($"{typeof(TEnum).Name}_{value}");
    }

    public string? TranslateEnum<TEnum>(TEnum? value)
        where TEnum : struct
    {
        return value == null
            ? null
            : TranslateEnum(value.Value);
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


    public IDictionary<string, string> GetAllTranslations(CultureInfo culture)
    {
        var dict = new Dictionary<string, string>();

        foreach (var resourceManager in _namespaceToResources.Values)
        {
            var keys = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, false, true)
                                      ?.OfType<DictionaryEntry>()
                                      .ToDictionary(entry => entry.Key, entry => entry.Value)
                                      .Select(entry => entry.Key)
                                      .OfType<string>()
                    ?? [];

            foreach (var key in keys)
            {
                dict.Add(key, resourceManager.GetString(key, culture) ?? key);
            }
        }

        return dict;
    }
}