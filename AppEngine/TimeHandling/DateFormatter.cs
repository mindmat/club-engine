using AppEngine.Properties;

namespace AppEngine.TimeHandling;

public class DateFormatter(RequestTimeProvider timeProvider)
{
    public string? GetEndText(IDatePeriod period)
    {
        if (period.From > timeProvider.RequestToday
         || period.Until == DateOnly.MaxValue
         || period.Until < timeProvider.RequestToday)
        {
            return null;
        }

        return string.Format(Resources.Until, period.Until.ToString("dd.MM.yyyy"));
    }
}