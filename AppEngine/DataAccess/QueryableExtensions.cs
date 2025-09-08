using System.Linq.Expressions;

using AppEngine.TimeHandling;

namespace AppEngine.DataAccess;

public static class QueryableExtensions
{
    public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source,
                                                       bool condition,
                                                       Expression<Func<TSource, bool>> predicate)
    {
        return condition
            ? source.Where(predicate)
            : source;
    }

    public static IQueryable<TSource> TakeIf<TSource>(this IQueryable<TSource> source,
                                                      bool condition,
                                                      int count)
    {
        return condition
            ? source.Take(count)
            : source;
    }

    public static IQueryable<TSource> WhereNotNull<TSource>(this IQueryable<TSource?> source)
        where TSource : struct
    {
        return source.Where(x => x != null)
                     .Select(x => x!.Value);
    }

    public static IQueryable<TSource> IsValidAt<TSource>(this IQueryable<TSource> source, DateOnly date)
        where TSource : IDatePeriod
    {
        return source.Where(x => x.From <= date
                              && x.Until >= date);
    }

    public static IQueryable<TSource> Overlaps<TSource>(this IQueryable<TSource> source, IDatePeriod period)
        where TSource : IDatePeriod
    {
        return source.Where(x => x.From <= period.Until
                              && x.Until >= period.From);
    }
}