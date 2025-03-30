namespace AppEngine.Types;

public static class StringExtensions
{
    public static string StringJoin(this IEnumerable<string> strings, string separator)
    {
        return string.Join(separator, strings);
    }
}