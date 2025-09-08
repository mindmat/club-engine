using AppEngine.Authorization;
using AppEngine.Configurations;
using AppEngine.Mailing.ExternalMails;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Mailing.Configuration;

public class ExternalMailConfigurationQuery : IRequest<IEnumerable<ExternalMailConfigurationDisplayItem>>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
}

public class ExternalMailConfigurationDisplayItem
{
    public Guid Id { get; set; }
    public string? ImapHost { get; set; }
    public int ImapPort { get; set; }
    public string? Username { get; set; }
    public bool PasswordSet { get; set; }
    public DateTime? ImportMailsSince { get; set; }

    public bool? CheckSuccessful { get; set; }
    public string? CheckError { get; set; }
    public int TotalImportedMails { get; set; }
    public int TotalAssignedMails { get; set; }
}

public class ExternalMailConfigurationQueryHandler(ConfigurationRegistry configurationRegistry,
                                                   IQueryable<ExternalMail> externalMails)
    : IRequestHandler<ExternalMailConfigurationQuery, IEnumerable<ExternalMailConfigurationDisplayItem>>
{
    public async Task<IEnumerable<ExternalMailConfigurationDisplayItem>> Handle(ExternalMailConfigurationQuery query, CancellationToken cancellationToken)
    {
        var configs = configurationRegistry.GetConfiguration<ExternalMailConfigurations>(query.PartitionId)
                                           .MailConfigurations?
                                           .Select(cfg => new ExternalMailConfigurationDisplayItem
                                                          {
                                                              Id = cfg.Id,
                                                              ImapHost = cfg.ImapHost,
                                                              ImapPort = cfg.ImapPort,
                                                              Username = cfg.Username,
                                                              PasswordSet = cfg.Password != null,
                                                              ImportMailsSince = cfg.ImportMailsSince,

                                                              CheckSuccessful = cfg.CheckSuccessful,
                                                              CheckError = cfg.CheckError
                                                          })
                                           .ToList()
                   ?? [];

        //var configIds = configs.Select(cfg => (Guid?)cfg.Id)
        //                       .ToList();

        //var imported = await externalMails.Where(iml => configIds.Contains(iml.ExternalMailConfigurationId))
        //                                  .Select(iml => new
        //                                                 {
        //                                                     iml.ExternalMailConfigurationId,
        //                                                     //Assigned = iml.Registrations!.Any()
        //                                                 })
        //                                  .ToListAsync(cancellationToken);

        //var importedCounts = imported.GroupBy(iml => iml.ExternalMailConfigurationId)
        //                             .Select(grp => new
        //                                            {
        //                                                Id = grp.Key,
        //                                                Total = grp.Count(),
        //                                                Assigned = false //grp.Count(iml => iml.Assigned)
        //                                            });

        //foreach (var config in configs)
        //{
        //    var counts = importedCounts.FirstOrDefault(cnt => cnt.Id == config.Id);
        //    config.TotalImportedMails = counts?.Total ?? 0;
        //    config.TotalAssignedMails = 0; //counts?.Assigned ?? 0;
        //}

        return configs;
    }
}