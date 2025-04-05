namespace AppEngine.Authorization;

public interface IPartitionBoundRequest
{
    Guid PartitionId { get; }
}