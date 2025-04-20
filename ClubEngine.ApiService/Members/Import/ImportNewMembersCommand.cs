using AppEngine.Authorization;
using AppEngine.DataAccess;

using ClubEngine.ApiService.Members.Memberships;

using MediatR;

namespace ClubEngine.ApiService.Members.Import;

public class ImportNewMembersCommand : IRequest, IPartitionBoundRequest
{
    public Guid PartitionId { get; set; }
    public IEnumerable<ImportedMember>? NewMembers { get; set; }
}

public class ImportNewMembersCommandHandler(IRepository<Member> members) : IRequestHandler<ImportNewMembersCommand>
{
    public Task Handle(ImportNewMembersCommand command, CancellationToken cancellationToken)
    {
        foreach (var newMember in command.NewMembers ?? [])
        {
            members.Insert(new Member
                           {
                               Id = newMember.Id,
                               ClubId = command.PartitionId,
                               FirstName = newMember.FirstName,
                               LastName = newMember.LastName,
                               Email = newMember.Email,
                               Address = newMember.Address,
                               Zip = newMember.Zip,
                               Town = newMember.Town,
                               Phone = newMember.Phone,
                               Memberships =
                               [
                                   new Membership
                                   {
                                       MembershipTypeId = newMember.MembershipTypeId,
                                       From = newMember.From,
                                       Until = newMember.Until
                                   }
                               ]
                           }
            );
        }

        return Task.CompletedTask;
    }
}