namespace AppEngine.Types;

public static class DictionaryExtensions
{
    public static TValue Lookup<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default!)
    {
        return dictionary.TryGetValue(key, out var value)
            ? value
            : defaultValue;
    }
}