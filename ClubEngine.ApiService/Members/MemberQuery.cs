using AppEngine.Authorization;
using AppEngine.MenuNodes;
using AppEngine.ReadModels;

using ClubEngine.ApiService.MembershipFees;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.Members;

public class MemberQuery : IRequest<MemberDetails>, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public Guid MemberId { get; set; }
}

public class MemberQueryHandler(ReadModelReader readModelReader) : IRequestHandler<MemberQuery, MemberDetails>
{
    public async Task<MemberDetails> Handle(MemberQuery query, CancellationToken cancellationToken)
    {
        return await readModelReader.GetDeserialized<MemberDetails>(nameof(MemberQuery),
                                                                    query.PartitionId,
                                                                    query.MemberId,
                                                                    cancellationToken);
    }
}

public class MemberNodeKey : IMenuNodeKey;

public class MemberCalculator(IQueryable<Member> members)
    : ReadModelCalculator<MemberDetails>
{
    public override string QueryName => nameof(MemberQuery);
    public override bool IsDateDependent => true;

    protected override async Task<(MemberDetails ReadModel, MenuNodeCalculation? MenuNode)> CalculateTyped(Guid partitionId, Guid? rowId, CancellationToken cancellationToken)
    {
        var member = await members.Where(mbr => mbr.ClubId == partitionId
                                             && mbr.Id == rowId)
                                  .Select(mbr => new
                                                 {
                                                     mbr.Id,
                                                     mbr.FirstName,
                                                     mbr.LastName,
                                                     mbr.Email,
                                                     mbr.Phone,
                                                     Fees = mbr.Fees!
                                                               .OrderByDescending(mfe => mfe.Period!.From)
                                                               .Select(mfe => new
                                                                              {
                                                                                  mfe.Period!.From,
                                                                                  mfe.Period.Until,
                                                                                  mfe.Amount,
                                                                                  mfe.AmountPaid_ReadModel,
                                                                                  mfe.State
                                                                              }),
                                                     LastMembership = mbr.Memberships!
                                                                         .OrderByDescending(mst => mst.From)
                                                                         .Select(mst => new
                                                                                        {
                                                                                            mst.From,
                                                                                            mst.Until,
                                                                                            mst.AnnualFeeOverride,
                                                                                            mst.MembershipTypeId
                                                                                        })
                                                                         .FirstOrDefault()
                                                 })
                                  .FirstAsync(cancellationToken);


        var readModel = new MemberDetails(member.Id,
                                          string.Join(", ", member.FirstName, member.LastName),
                                          member.Email,
                                          member.Phone,
                                          member.LastMembership == null
                                              ? null
                                              : new MembershipDetails(member.LastMembership.MembershipTypeId,
                                                                      member.LastMembership.From,
                                                                      member.LastMembership.Until),
                                          member.Fees.Select(fee => new FeeDetails(fee.From,
                                                                                   fee.Until,
                                                                                   fee.Amount,
                                                                                   fee.Amount - fee.AmountPaid_ReadModel,
                                                                                   fee.State)));

        return (readModel, null);
    }
}

public record MemberDetails(Guid Id, string? Name, string? Email, string? Phone, MembershipDetails? LastMembership, IEnumerable<FeeDetails> Fees);

public record MembershipDetails(Guid MembershipTypeId, DateOnly From, DateOnly Until);

public record FeeDetails(DateOnly PeriodFrom, DateOnly PeriodUntil, decimal Amount, decimal AmountOpen, MembershipFeeState State);