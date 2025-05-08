using AppEngine.DataAccess;
using AppEngine.Internationalization;
using AppEngine.Partitions;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Authorization.UsersInPartition;

public class MyPartitionsQuery : IRequest<MyPartitions>
{
    public string? SearchString { get; set; }
    public bool ShowArchived { get; set; }
}

public class MyPartitionsQueryHandler(IQueryable<IPartition> partitions,
                                      AuthenticatedUserId authenticatedUserId,
                                      Translator translator)
    : IRequestHandler<MyPartitionsQuery, MyPartitions>
{
    public async Task<MyPartitions> Handle(MyPartitionsQuery query,
                                           CancellationToken cancellationToken)
    {
        var data = await partitions.WhereIf(!string.IsNullOrEmpty(query.SearchString),
                                            evt => EF.Functions.Like(evt.Name, $"%{query.SearchString}%")
                                                || EF.Functions.Like(evt.Acronym, $"%{query.SearchString}%"))
                                   .WhereIf(!query.ShowArchived, prt => !prt.IsArchived)
                                   .Select(prt => new
                                                  {
                                                      prt.Id,
                                                      prt.Name,
                                                      prt.Acronym,
                                                      prt.IsArchived,
                                                      RoleInPartition = prt.Users!
                                                                           .Where(uip => uip.UserId == authenticatedUserId.UserId)
                                                                           .Select(uip => (UserInPartitionRole?)uip.Role)
                                                                           .FirstOrDefault(),
                                                      AccessRequest = prt.AccessRequests!
                                                                         .FirstOrDefault(arq => arq.UserId_Requestor == authenticatedUserId.UserId)
                                                  })
                                   .ToListAsync(cancellationToken);

        return new MyPartitions
               {
                   Authorized = data.Where(prt => prt.RoleInPartition != null)
                                    .OrderBy(uip => uip.IsArchived)
                                    .ThenBy(prt => prt.Name)
                                    .Select(uip => new PartitionOfUser(uip.Id,
                                                                       uip.Name,
                                                                       uip.Acronym,
                                                                       uip.RoleInPartition!.Value,
                                                                       translator.TranslateEnum(uip.RoleInPartition!.Value)))
                                    .ToList(),

                   Requests = data.Where(prt => prt.RoleInPartition == null
                                             && prt.AccessRequest != null)
                                  .Select(req => new AccessRequest(req.Id,
                                                                   req.Name,
                                                                   req.Acronym,
                                                                   req.AccessRequest!.RequestReceived))
                                  .ToList(),

                   Other = data.Where(prt => prt.RoleInPartition == null
                                          && prt.AccessRequest == null)
                               .OrderBy(uip => uip.IsArchived)
                               .ThenBy(prt => prt.Name)
                               .Select(uip => new PartitionDisplayItem(uip.Id,
                                                                       uip.Name,
                                                                       uip.Acronym))
                               .ToList(),
               };
    }
}

public class MyPartitions
{
    public IEnumerable<PartitionOfUser> Authorized { get; set; } = null!;
    public IEnumerable<AccessRequest> Requests { get; set; } = null!;
    public IEnumerable<PartitionDisplayItem> Other { get; set; } = null!;
}

public record PartitionDisplayItem(Guid Id,
                                   string Name,
                                   string Acronym);

public record PartitionOfUser(Guid Id,
                              string Name,
                              string Acronym,
                              UserInPartitionRole Role,
                              string RoleText);

public record AccessRequest(Guid PartitionId,
                            string PartitionName,
                            string PartitionAcronym,
                            DateTimeOffset RequestSent);