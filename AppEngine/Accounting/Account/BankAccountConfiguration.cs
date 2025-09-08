using AppEngine.Configurations;

namespace AppEngine.Accounting.Account;

public class BankAccountConfiguration : IConfigurationItem
{
    public string? Iban { get; set; }
    public string? AccountHolderName { get; set; }
    public string? AccountHolderStreet { get; set; }
    public string? AccountHolderHouseNo { get; set; }
    public string? AccountHolderPostalCode { get; set; }
    public string? AccountHolderTown { get; set; }
    public string? AccountHolderCountryCode { get; set; }
}

public class DefaultBankAccountConfiguration : BankAccountConfiguration, IDefaultConfigurationItem
{
}