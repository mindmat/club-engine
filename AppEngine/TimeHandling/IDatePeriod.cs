namespace AppEngine.TimeHandling;

public interface IDatePeriod
{
    public DateOnly From { get; }
    public DateOnly Until { get; }
}

public static class DatePeriodExtensions
{
    public static bool IsActiveAt(this IDatePeriod period, DateOnly date)
    {
        return period.From <= date
            && period.Until >= date;
    }
}