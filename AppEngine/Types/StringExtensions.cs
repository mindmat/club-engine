namespace AppEngine.Types;

public static class StringExtensions
{
    public static string StringJoin(this IEnumerable<string> strings, string separator)
    {
        return string.Join(separator, strings);
    }

    public static DateOnly ToDate(this string? value, string? mappingFormat, DateOnly fallback)
    {
        if (value != null)
        {
            if (mappingFormat != null
             && DateOnly.TryParseExact(value, mappingFormat, out var date))
            {
                return date;
            }

            if (DateOnly.TryParse(value, out date))
            {
                return date;
            }
        }

        return fallback;
    }

    public static IEnumerable<string> WhereNotNull(this IEnumerable<string?> source)
    {
        return source.Where(x => x != null)
                     .Select(x => x!);
    }
}