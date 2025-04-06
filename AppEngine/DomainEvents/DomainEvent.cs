namespace AppEngine.DomainEvents;

public class DomainEvent
{
    public Guid? DomainEventId_Parent { get; set; }
    public Guid? PartitionId { get; set; }
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
}