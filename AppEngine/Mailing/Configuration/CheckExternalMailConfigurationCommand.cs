using AppEngine.Authorization;
using AppEngine.Configurations;
using AppEngine.ReadModels;

using MailKit;
using MailKit.Net.Imap;

using MediatR;

namespace AppEngine.Mailing.Configuration;

public class CheckExternalMailConfigurationCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public Guid ExternalMailConfigurationId { get; set; }
}

public class CheckExternalMailConfigurationCommandHandler(ConfigurationRegistry configurationRegistry,
                                                          ChangeTrigger changeTrigger) : IRequestHandler<CheckExternalMailConfigurationCommand>
{
    public async Task Handle(CheckExternalMailConfigurationCommand command, CancellationToken cancellationToken)
    {
        var mailConfigurations = configurationRegistry.GetConfiguration<ExternalMailConfigurations>(command.PartitionId);
        var config = mailConfigurations.MailConfigurations?.FirstOrDefault(cfg => cfg.Id == command.ExternalMailConfigurationId);

        if (config == null)
        {
            return;
        }

        using var client = new ImapClient();

        client.ServerCertificateValidationCallback = (_, _, _, _) => true;

        try
        {
            await client.ConnectAsync(config.ImapHost, config.ImapPort, true, cancellationToken);

            try
            {
                await client.AuthenticateAsync(config.Username, config.Password, cancellationToken);

                try
                {
                    // The Inbox folder is always available on all IMAP servers
                    await client.Inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
                    config.CheckSuccessful = true;
                    config.CheckError = null;
                }
                catch (Exception ex)
                {
                    config.CheckSuccessful = false;
                    config.CheckError = $"Could not open inbox. Error: {ex.Message}";
                    await configurationRegistry.UpdateConfiguration(command.PartitionId, mailConfigurations);
                }
            }
            catch (Exception ex)
            {
                config.CheckSuccessful = false;
                config.CheckError = $"Could not authenticate user {config.Username}. Error: {ex.Message}";
                await configurationRegistry.UpdateConfiguration(command.PartitionId, mailConfigurations);
            }
        }
        catch (Exception ex)
        {
            config.CheckSuccessful = false;
            config.CheckError = $"Could not connect to {config.ImapHost}:{config.ImapPort}. Error: {ex.Message}";
            await configurationRegistry.UpdateConfiguration(command.PartitionId, mailConfigurations);
        }

        await configurationRegistry.UpdateConfiguration(command.PartitionId, mailConfigurations);

        changeTrigger.QueryChanged<ExternalMailConfigurationQuery>(command.PartitionId);
    }
}