namespace AppEngine.Partitions;

public class PartitionContext
{
    public Guid? PartitionId { get; set; }

    public static implicit operator Guid?(PartitionContext partitionContext)
    {
        return partitionContext.PartitionId;
    }
}