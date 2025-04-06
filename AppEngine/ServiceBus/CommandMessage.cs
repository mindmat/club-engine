namespace AppEngine.ServiceBus;

public record CommandMessage(string? CommandType, string? CommandSerialized, TimeSpan? Delay);