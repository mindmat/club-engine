namespace AppEngine.ServiceBus;

public record ServiceBusConsumer(string QueueName, Type RequestType);