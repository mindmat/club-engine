using AppEngine.Accounting.Assignments;
using AppEngine.DataAccess;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.MembershipFees;

public class FeesSource(IQueryable<MembershipFee> fees) : IPaymentAssignmentSource
{
    public const string SourceType = "MembershipFees";
    public string Type => SourceType;

    public async Task<IDictionary<Guid, SourceInfos>> GetSourceInfos(Guid partitionId, IEnumerable<Guid> sourceIds)
    {
        return await fees.Where(fee => fee.Member!.ClubId == partitionId
                                    && sourceIds.Contains(fee.Id))
                         .Select(fee => new
                                        {
                                            fee.Id,
                                            fee.Member!.FirstName,
                                            fee.Member.LastName,
                                            fee.Member.Email
                                        })
                         .ToDictionaryAsync(fee => fee.Id,
                                            fee => new SourceInfos
                                                   {
                                                       TextPrimary = $"{fee.FirstName} {fee.LastName}",
                                                       TextSecondary = fee.Email
                                                   });
    }

    public async Task<IEnumerable<SourceAssignmentCandidate>> GetCandidates(Guid partitionId,
                                                                            string? searchString,
                                                                            string? paymentMessage,
                                                                            string? paymentOtherParty,
                                                                            CancellationToken cancellationToken)
    {
        var userSearch = !string.IsNullOrEmpty(searchString);

        return await fees.Where(msf => msf.Period!.ClubId == partitionId
                                    && msf.State == MembershipFeeState.Due)
                         .WhereIf(userSearch,
                                  msf => EF.Functions.Like(msf.Member!.FirstName!, $"%{searchString}%")
                                      || EF.Functions.Like(msf.Member!.LastName!, $"%{searchString}%")
                                      || EF.Functions.Like(msf.Member!.Email!, $"%{searchString}%"))
                         .WhereIf(!userSearch,
                                  reg => paymentMessage != null && reg.Member!.FirstName != null && paymentMessage.Contains(reg.Member!.FirstName!)
                                      || paymentMessage != null && reg.Member!.LastName != null && paymentMessage.Contains(reg.Member!.FirstName!)
                                      || paymentMessage != null && reg.Member!.Email != null && paymentMessage.Contains(reg.Member!.FirstName!)
                                      || paymentOtherParty != null && reg.Member!.FirstName != null && paymentOtherParty.Contains(reg.Member!.FirstName!)
                                      || paymentOtherParty != null && reg.Member!.FirstName != null && paymentOtherParty.Contains(reg.Member!.FirstName!))
                         .Select(msf => new SourceAssignmentCandidate
                                        {
                                            SourceId = msf.Id,
                                            TextPrimary = msf.Member!.FirstName + " " + msf.Member.LastName,
                                            TextSecondary = msf.Member.Email,
                                            AmountTotal = msf.Amount,
                                        })
                         .ToListAsync(cancellationToken);
    }
}