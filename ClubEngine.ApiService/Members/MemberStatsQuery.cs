using AppEngine.Authorization;
using AppEngine.ReadModels;

namespace ClubEngine.ApiService.Members;

public class MemberStatsQuery : IRequest<MemberStats>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
}

public class MemberStatsQueryHandler(ReadModelReader readModelReader) : IRequestHandler<MemberStatsQuery, MemberStats>
{
    public async Task<MemberStats> Handle(MemberStatsQuery query, CancellationToken cancellationToken)
    {
        return await readModelReader.GetDeserialized<MemberStats>(nameof(MemberStatsQuery),
                                                                  query.PartitionId,
                                                                  (Guid?)null,
                                                                  cancellationToken);
    }
}