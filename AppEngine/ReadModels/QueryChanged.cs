using AppEngine.DomainEvents;

namespace AppEngine.ReadModels;

public class QueryChanged : DomainEvent
{
    public string QueryName { get; set; } = null!;
    public Guid? RowId { get; set; }
}