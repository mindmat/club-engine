using System.Globalization;

using AppEngine.Authorization;
using AppEngine.DataAccess;
using AppEngine.Mediator;

using CsvHelper;
using CsvHelper.Configuration;

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
        var fileType = query.File.ContentType == "text/csv"
            ? MemberListImportConfig.FileType.Csv
            : MemberListImportConfig.FileType.Csv;
        var sourceConfig = config.Sources.Where(src => src.FileType == fileType);

        var existingMembers = await members.Where(mbr => mbr.ClubId == query.PartitionId)
                                           .ToListAsync(cancellationToken);

        foreach (var importConfig in sourceConfig)
        {
            try
            {
                var membersInFile = await ParseCsv(query, importConfig);
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

    private (IEnumerable<ImportedMember> added, IEnumerable<Member> modified, IEnumerable<ImportedMember> deleted) CompareMembers(List<Member> existingMembers, List<ImportedMember> membersInFile)
    {
        var added = membersInFile.Where(nwm => !existingMembers.Any(exm => IsSame(exm, nwm)))
                                 .ToList();

        var deleted = existingMembers.Where(exm => membersInFile.Any(nwm => IsSame(exm, nwm)))
                                     .Select(exm => new ImportedMember { Id = Guid.NewGuid(), FirstName = exm.FirstName, LastName = exm.LastName, Email = exm.Email })
                                     .ToList();

        return (added, [], deleted);
    }

    private bool IsSame(Member existingMember, ImportedMember importedMember)
    {
        if (string.Equals(existingMember.FirstName, importedMember.FirstName, StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(existingMember.LastName, importedMember.LastName, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static async Task<List<ImportedMember>> ParseCsv(ImportMemberListQuery query, MemberListImportConfig.Source importConfig)
    {
        using var reader = new StreamReader(query.File.FileStream);

        using var csv = new CsvReader(reader,
                                      new CsvConfiguration(CultureInfo.InvariantCulture)
                                      {
                                          HasHeaderRecord = true,
                                      });

        var headerMappings = importConfig.Mappings.ToDictionary(m => m.Header, m => m);

        var members = new List<ImportedMember>();
        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? [];

        while (await csv.ReadAsync())
        {
            var member = new ImportedMember { Id = Guid.NewGuid() };

            foreach (var header in headers)
            {
                if (!headerMappings.TryGetValue(header, out var mapping)) continue;

                switch (mapping.OurColumn)
                {
                    case MemberListImportConfig.OurColumn.FirstName:
                        member.FirstName = csv.GetField(header);

                        break;

                    case MemberListImportConfig.OurColumn.LastName:
                        member.LastName = csv.GetField(header);

                        break;

                    case MemberListImportConfig.OurColumn.Email:
                        member.Email = csv.GetField(header);

                        break;

                    case MemberListImportConfig.OurColumn.Tag:
                        var tagged = csv.GetField(header) == (mapping.TagActive ?? "true");

                        if (tagged)
                        {
                            member.Tags.Add(mapping.Tag ?? header);
                        }

                        break;

                    default:
                        break;
                }
            }

            members.Add(member);
        }

        return members;
    }
}