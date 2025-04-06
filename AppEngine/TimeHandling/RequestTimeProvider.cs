namespace AppEngine.TimeHandling;

public class RequestTimeProvider(TimeProvider timeProvider)
{
    public DateTimeOffset RequestTime { get; init; } = timeProvider.GetLocalNow();
}