﻿namespace AppEngine.Types;

public static class EnumerableExtensions
{
    public static IEnumerable<TSource> FillUpIf<TSource>(this IEnumerable<TSource> source, int? minLength, Func<TSource> createFillElement)
    {
        return minLength != null
            ? FillUp(source, createFillElement, minLength.Value)
            : source;
    }

    public static IEnumerable<TSource> FillUp<TSource>(this IEnumerable<TSource> source, Func<TSource> createFillElement, int minLength)
    {
        var list = source.ToList();
        IEnumerable<TSource> filledList = list;
        var count = list.Count;

        for (var i = count; i < minLength; i++)
        {
            filledList = filledList.Append(createFillElement());
        }

        return filledList;
    }

    public static IEnumerable<TSource> AppendIf<TSource>(this IEnumerable<TSource> source, bool condition, Func<TSource> createElement)
    {
        return condition
            ? source.Append(createElement.Invoke())
            : source;
    }

    public static async Task ForEach<T>(this IEnumerable<T> source, Func<T, Task> action)
    {
        foreach (var obj in source)
        {
            await action(obj);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var obj in source)
        {
            action(obj);
        }
    }

    public static IEnumerable<TSource> WhereIf<TSource>(this IEnumerable<TSource> source,
                                                        bool condition,
                                                        Func<TSource, bool> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }

    public static IEnumerable<TSource> WhereNotNull<TSource>(this IEnumerable<TSource?> source)
        where TSource : class
    {
        return source.Where(x => x != null)
                     .Select(x => x!);
    }

    public static IEnumerable<TSource> WhereNotNull<TSource>(this IEnumerable<TSource?> source)
        where TSource : struct
    {
        return source.Where(x => x != null)
                     .Select(x => x!.Value);
    }

    public static IList<TSource> AsList<TSource>(this IEnumerable<TSource> source)
    {
        return source as IList<TSource> ?? source.ToList();
    }

    public static IEnumerable<TSource> TakeIf<TSource>(this IEnumerable<TSource> source,
                                                       bool condition,
                                                       int count)
    {
        return condition
            ? source.Take(count)
            : source;
    }

    public static async Task<IEnumerable<T>> SelectManyAsync<TSource, T>(this IEnumerable<TSource> source,
                                                                         Func<TSource, Task<IEnumerable<T>>> projection)
    {
        IEnumerable<T> result = [];

        foreach (var item in source)
        {
            var localProjection = await projection(item);
            result = result.Concat(localProjection);
        }

        return result;
    }
}