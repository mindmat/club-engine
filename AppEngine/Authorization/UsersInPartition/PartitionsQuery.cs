using MediatR;

using Microsoft.AspNetCore.Authorization;

namespace AppEngine.Authorization.UsersInPartition;

[AllowAnonymous]
public class PartitionsQuery : IRequest<MyPartitions>
{
    public string? SearchString { get; set; }
    public bool ShowArchived { get; set; }
}

public class PartitionsQueryHandler(IRequestHandler<MyPartitionsQuery, MyPartitions> qh) : IRequestHandler<PartitionsQuery, MyPartitions>
{
    public Task<MyPartitions> Handle(PartitionsQuery query, CancellationToken cancellationToken)
    {
        return qh.Handle(new MyPartitionsQuery
                         {
                             SearchString = query.SearchString,
                             ShowArchived = query.ShowArchived
                         },
                         cancellationToken);
    }
}