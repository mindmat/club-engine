using Microsoft.EntityFrameworkCore;

namespace AppEngine.Partitions;

public class PartitionByAcronymQuery : IRequest<PartitionDetails?>
{
    public string? Acronym { get; set; }
}

public class PartitionByAcronymQueryHandler(IQueryable<IPartition> partitions) : IRequestHandler<PartitionByAcronymQuery, PartitionDetails?>
{
    public async Task<PartitionDetails?> Handle(PartitionByAcronymQuery query, CancellationToken cancellationToken)
    {
        if (query.Acronym == null)
        {
            return null;
        }

        var partition = await partitions.FirstAsync(evt => evt.Acronym == query.Acronym.ToLowerInvariant(), cancellationToken);

        return new PartitionDetails
               {
                   Id = partition.Id,
                   Name = partition.Name,
                   Acronym = partition.Acronym
               };
    }
}

public class PartitionDetails
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Acronym { get; set; }
}