using AppEngine.DomainEvents;

namespace AppEngine.Accounting.Iso20022.Camt;

public class PaymentFileProcessed : DomainEvent
{
    public string? Account { get; set; }
    public decimal Balance { get; set; }
    public int EntriesCount { get; set; }
}

public class PaymentFileProcessedUserTranslation : IEventToUserTranslation<PaymentFileProcessed>
{
    public string GetText(PaymentFileProcessed domainEvent)
    {
        return $"{domainEvent.EntriesCount} Kontobewegungen auf {domainEvent.Account}, Saldo {domainEvent.Balance}";
    }
}