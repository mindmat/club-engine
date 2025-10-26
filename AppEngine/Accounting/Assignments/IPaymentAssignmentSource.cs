namespace AppEngine.Accounting.Assignments;

public interface IPaymentAssignmentSource
{
    string Type { get; }

    Task<IDictionary<Guid, SourceInfos>> GetSourceInfos(Guid partitionId, IEnumerable<Guid> sourceIds);

    Task<IEnumerable<SourceAssignmentCandidate>> GetCandidates(Guid partitionId,
                                                               string? searchString,
                                                               string? paymentMessage,
                                                               string? paymentOtherParty, CancellationToken cancellationToken);
}

public class SourceInfos
{
    public string? TextPrimary { get; set; }
    public string? TextSecondary { get; set; }
    public string[]? Tags { get; set; }
}

public class SourceAssignmentCandidate
{
    public Guid SourceId { get; set; }
    public string? TextPrimary { get; set; }
    public string? TextSecondary { get; set; }
    public string[] Tags { get; set; }
    public decimal AmountTotal { get; set; }
    public decimal AmountOpen { get; set; }
    public int MatchScore { get; set; }
}