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

    public string GetPeriodText(IDatePeriod period)
    {
        // Helper for month name
        string GetMonthName(DateOnly date) => date.ToString("MMMM", System.Globalization.CultureInfo.CurrentCulture);

        if (period.From is { Day: 1, Month: 1 } && period.Until is { Day: 31, Month: 12 })
        {
            if (period.From.Year == period.Until.Year)
            {
                // Full year (e.g. 01.01.2025 - 31.12.2025)
                return period.From.Year.ToString();
            }

            // Full years (e.g. 01.01.2025 - 31.12.2026)
            return $"{period.From.Year} - {period.Until.Year}";
        }

        if (period.From.Day == 1 && period.Until.Day == DateTime.DaysInMonth(period.Until.Year, period.Until.Month))
        {
            if (period.From.Year == period.Until.Year)
            {
                // Full months, same year (e.g. 01.01.2025 - 30.11.2025)
                return $"{GetMonthName(period.From)} - {GetMonthName(period.Until)} {period.From.Year}";
            }

            // Full months, different years (e.g. 01.01.2025 - 30.11.2026)
            return $"{GetMonthName(period.From)} {period.From.Year} - {GetMonthName(period.Until)} {period.Until.Year}";
        }

        // Otherwise, show full date range
        return $"{period.From.ToString("dd.MM.yyyy")} - {period.Until.ToString("dd.MM.yyyy")}";
    }
}