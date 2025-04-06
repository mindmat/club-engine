using AppEngine.Json;
using AppEngine.TimeHandling;

using Azure.Messaging.ServiceBus;

using MediatR;

namespace AppEngine.ServiceBus;

public class CommandQueue(ServiceBusSender sender, RequestTimeProvider timeProvider, Serializer serializer)
{
    public const string CommandQueueName = "CommandQueue";
    private readonly List<EnqueuedCommand> _messages = [];

    public async Task Release(bool dbCommitSucceeded)
    {
        if (!_messages.Any())
        {
            return;
        }

        foreach (var chunkOfMessages in _messages.Where(msg => dbCommitSucceeded || msg.SendAnyway)
                                                 .Where(msg => msg.Delay == null)
                                                 .Chunk(100))
        {
            await sender.SendMessagesAsync(chunkOfMessages.Select(msg => new ServiceBusMessage(serializer.Serialize(msg.Message))));
        }

        var delayedGroups = _messages.Where(msg => msg.Delay != null)
                                     .GroupBy(msg => msg.Delay!.Value);
        foreach (var delayedGroup in delayedGroups)
        {
            var scheduledAt = timeProvider.RequestTime + delayedGroup.Key;
            foreach (var delayedChunk in delayedGroup.Chunk(100))
            {
                await sender.ScheduleMessagesAsync(delayedChunk.Select(msg => new ServiceBusMessage(serializer.Serialize(msg.Message))), scheduledAt);
            }
        }
    }

    public void EnqueueCommand<T>(T command,
                                  bool publishEvenWhenDbCommitFails = false,
                                  TimeSpan? delay = null)
        where T : IRequest
    {
        var commandSerialized = serializer.Serialize(command);
        _messages.Add(new EnqueuedCommand
                      (
                          new CommandMessage(CommandType: command.GetType().FullName!, CommandSerialized: commandSerialized, Delay: delay),
                          publishEvenWhenDbCommitFails,
                          delay));
    }

    private record EnqueuedCommand(CommandMessage Message, bool SendAnyway, TimeSpan? Delay);
}