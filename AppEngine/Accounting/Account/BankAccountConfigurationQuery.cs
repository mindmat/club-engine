using AppEngine.Authorization;
using AppEngine.Configurations;

using MediatR;

namespace AppEngine.Accounting.Account;

public class BankAccountConfigurationQuery : IRequest<BankAccountConfiguration>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
}

public class BankAccountConfigurationQueryHandler(ConfigurationRegistry configurationRegistry)
    : IRequestHandler<BankAccountConfigurationQuery, BankAccountConfiguration>
{
    public Task<BankAccountConfiguration> Handle(BankAccountConfigurationQuery query, CancellationToken _)
    {
        return Task.FromResult(configurationRegistry.GetConfiguration<BankAccountConfiguration>(query.PartitionId));
    }
}