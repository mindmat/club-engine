using AppEngine.Authorization;
using AppEngine.Configurations;
using AppEngine.ReadModels;

using MediatR;

namespace AppEngine.Mailing.Configuration;

public class SaveExternalMailConfigurationCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public IEnumerable<ExternalMailConfigurationUpdateItem>? Configs { get; set; }
}

public class ExternalMailConfigurationUpdateItem
{
    public Guid Id { get; set; }
    public string? ImapHost { get; set; }
    public int ImapPort { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public DateTime? ImportMailsSince { get; set; }
}

public class SaveExternalMailConfigurationCommandHandler(ConfigurationRegistry configurationRegistry,
                                                         ChangeTrigger changeTrigger)
    : IRequestHandler<SaveExternalMailConfigurationCommand>
{
    public async Task Handle(SaveExternalMailConfigurationCommand command, CancellationToken cancellationToken)
    {
        var existingConfigs = configurationRegistry.GetConfiguration<ExternalMailConfigurations>(command.PartitionId)
                                                   .MailConfigurations;

        var newConfigs = command.Configs?
                                .Select(nwc =>
                                {
                                    var existing = existingConfigs?.FirstOrDefault(exc => exc.Id == nwc.Id);

                                    var config = new ExternalMailConfiguration
                                                 {
                                                     Id = nwc.Id,
                                                     ImapHost = nwc.ImapHost,
                                                     ImapPort = nwc.ImapPort,
                                                     Username = nwc.Username,
                                                     Password = string.IsNullOrWhiteSpace(nwc.Password)
                                                         ? existing?.Password
                                                         : nwc.Password,
                                                     ImportMailsSince = nwc.ImportMailsSince
                                                 };

                                    if (existing is { CheckSuccessful: not null })
                                    {
                                        config.CheckSuccessful = existing.CheckSuccessful;
                                        config.CheckError = existing.CheckError;
                                    }

                                    return config;
                                })
                                .ToList();

        await configurationRegistry.UpdateConfiguration(command.PartitionId,
                                                        new ExternalMailConfigurations
                                                        {
                                                            MailConfigurations = newConfigs
                                                        });

        changeTrigger.QueryChanged<ExternalMailConfigurationQuery>(command.PartitionId);

        foreach (var config in newConfigs?.Where(cfg => cfg is { ImapHost: not null, Username: not null })
                            ?? [])
        {
            changeTrigger.EnqueueCommand(new CheckExternalMailConfigurationCommand
                                         {
                                             PartitionId = command.PartitionId,
                                             ExternalMailConfigurationId = config.Id
                                         });
        }
    }
}