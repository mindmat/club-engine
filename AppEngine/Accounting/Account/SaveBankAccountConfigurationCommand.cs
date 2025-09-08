using AppEngine.Authorization;
using AppEngine.Configurations;
using AppEngine.ReadModels;

using MediatR;

namespace AppEngine.Accounting.Account;

public class SaveBankAccountConfigurationCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public BankAccountConfiguration? Config { get; set; }
}

public class SaveBankAccountConfigurationCommandHandler(ConfigurationRegistry configurationRegistry,
                                                        ChangeTrigger changeTrigger)
    : IRequestHandler<SaveBankAccountConfigurationCommand>
{
    public async Task Handle(SaveBankAccountConfigurationCommand command, CancellationToken cancellationToken)
    {
        if (command.Config == null)
        {
            throw new ArgumentNullException(nameof(command.Config));
        }

        await configurationRegistry.UpdateConfiguration(command.PartitionId,
                                                        command.Config);

        changeTrigger.QueryChanged<BankAccountConfigurationQuery>(command.PartitionId);
    }
}