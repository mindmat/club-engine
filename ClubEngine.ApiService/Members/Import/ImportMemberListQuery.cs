using AppEngine.Authorization;
using AppEngine.DataAccess;
using AppEngine.Mediator;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ClubEngine.ApiService.Members.Import;

public class ImportMemberListQuery : IRequest<ListDifferences>, IPartitionBoundRequest, IReceiveFileCommand
{
    public FileUpload File { get; set; } = null!;
    public Guid PartitionId { get; set; }
}

public record ListDifferences(IEnumerable<ImportedMember> Added, IEnumerable<ImportedMember> Modified, IEnumerable<ImportedMember> Deleted);

public class ImportMemberListCommandHandler(MemberListImportConfig config,
                                            IRepository<Member> members) : IRequestHandler<ImportMemberListQuery, ListDifferences>
{
    public async Task<ListDifferences> Handle(ImportMemberListQuery query, CancellationToken cancellationToken)
    {
        var fileType = query.File.ContentType switch
        {
            "text/csv"                                                          => MemberListImportConfig.FileType.Csv,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => MemberListImportConfig.FileType.Xlsx,
            _                                                                   => MemberListImportConfig.FileType.Unknown
        };
        var sourceConfig = config.Sources.Where(src => src.FileType == fileType);

        var existingMembers = await members.Where(mbr => mbr.ClubId == query.PartitionId)
                                           .ToListAsync(cancellationToken);

        foreach (var importConfig in sourceConfig)
        {
            try
            {
                var membersInFile = await Parse(query, importConfig);
                var (added, modified, deleted) = CompareMembers(existingMembers, membersInFile);

                return new ListDifferences(added, [], deleted);
            }
            catch
            {
                //  nop - sandbox
            }
        }

        return new ListDifferences([], [], []);
    }

    private async Task<List<ImportedMember>> Parse(ImportMemberListQuery query, MemberListImportConfig.Source importConfig)
    {
        return importConfig.FileType switch
        {
            MemberListImportConfig.FileType.Csv  => await CsvMembersParser.ParseCsv(query, importConfig.Mappings),
            MemberListImportConfig.FileType.Xlsx => XlsxMembersParser.ParseXlsx(query, importConfig.Mappings),
            _                                    => throw new ArgumentOutOfRangeException()
        };
    }

    private (IEnumerable<ImportedMember> added, IEnumerable<Member> modified, IEnumerable<ImportedMember> deleted) CompareMembers(List<Member> existingMembers, List<ImportedMember> membersInFile)
    {
        var matches = new List<Match>();
        var existingMembersNotMatched = new HashSet<Member>(existingMembers);

        foreach (var importedMember in membersInFile)
        {
            var existingMember = existingMembers.FirstOrDefault(exm => IsSame(exm, importedMember));

            if (existingMember != null)
            {
                existingMembersNotMatched.Remove(existingMember);
            }

            matches.Add(new Match(importedMember, existingMember));
        }

        matches.AddRange(existingMembersNotMatched.Select(exm => new Match(null, exm)));

        var added = matches.Where(mat => mat.Existing == null)
                           .Select(mat => mat.Imported!);

        var deleted = matches.Where(mat => mat.Imported == null)
                             .Select(mat => new ImportedMember
                                            {
                                                Id = mat.Existing!.Id,
                                                FirstName = mat.Existing.FirstName,
                                                LastName = mat.Existing.LastName,
                                                Email = mat.Existing.Email
                                            })
                             .ToList();

        return (added, [], deleted);
    }

    private record Match(ImportedMember? Imported, Member? Existing);

    private bool IsSame(Member existingMember, ImportedMember importedMember)
    {
        if (string.Equals(existingMember.FirstName, importedMember.FirstName, StringComparison.InvariantCultureIgnoreCase)
         && string.Equals(existingMember.LastName, importedMember.LastName, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        return false;
    }
}