namespace AppEngine.TimeHandling;

public class RequestTimeProvider(TimeProvider timeProvider)
{
    public DateTimeOffset RequestNow { get; init; } = timeProvider.GetLocalNow();
    public DateOnly RequestToday { get; init; } = DateOnly.FromDateTime(timeProvider.GetLocalNow().LocalDateTime);
}