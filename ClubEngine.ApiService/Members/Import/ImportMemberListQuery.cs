using System.Globalization;

using AppEngine.Authorization;
using AppEngine.DataAccess;
using AppEngine.Mediator;

using ClosedXML.Excel;

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
        var fileType = query.File.ContentType switch
        {
            "text/csv" => MemberListImportConfig.FileType.Csv,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => MemberListImportConfig.FileType.Xlsx,
            _ => MemberListImportConfig.FileType.Unknown
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
            MemberListImportConfig.FileType.Csv => await ParseCsv(query, importConfig.Mappings),
            MemberListImportConfig.FileType.Xlsx => ParseXlsx(query, importConfig.Mappings),
            _ => throw new ArgumentOutOfRangeException()
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
                             .Select(mat => new ImportedMember { Id = mat.Existing!.Id, FirstName = mat.Existing.FirstName, LastName = mat.Existing.LastName, Email = mat.Existing.Email })
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

    private static List<ImportedMember> ParseXlsx(ImportMemberListQuery query, MemberListImportConfig.ColumnMapping[] mappings)
    {
        using var workbook = new XLWorkbook(query.File.FileStream);

        var worksheet = workbook.Worksheet(1);
        var headerRow = worksheet.RowsUsed().First();

        var headers = headerRow.CellsUsed().Where(cell => cell.Value.IsText)
                               .ToDictionary(cell => cell.Value.GetText().ToLowerInvariant(),
                                             cell => cell.Address.ColumnNumber);

        var mappingsWithColumn = mappings.Select(map =>
                                         {
                                             if (headers.TryGetValue(map.Header.ToLowerInvariant(), out var column))
                                             {
                                                 return new
                                                 {
                                                     Column = column,
                                                     Mapping = map
                                                 };
                                             }

                                             return null;
                                         })
                                         .Where(map => map != null)
                                         .ToList();

        var members = new List<ImportedMember>();

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            var member = new ImportedMember
            {
                Id = Guid.NewGuid(),
                MemberFrom = DateOnly.MinValue,
                MemberUntil = DateOnly.MaxValue
            };

            foreach (var header in mappingsWithColumn)
            {
                var cell = row.Cell(header!.Column);

                switch (header.Mapping.OurColumn)
                {
                    case MemberListImportConfig.OurColumn.FirstName:
                        member.FirstName = GetText(cell);

                        break;

                    case MemberListImportConfig.OurColumn.LastName:
                        member.LastName = GetText(cell);

                        break;

                    case MemberListImportConfig.OurColumn.Email:
                        member.Email = GetText(cell);

                        break;

                    //case MemberListImportConfig.OurColumn.Tag:
                    //    var tagged = csv.GetField(header) == (mapping.TagActive ?? "true");

                    //    if (tagged)
                    //    {
                    //        member.Tags.Add(mapping.Tag ?? header);
                    //    }

                    //    break;

                    case MemberListImportConfig.OurColumn.Address:
                        member.Address = GetText(cell);

                        break;

                    case MemberListImportConfig.OurColumn.Zip:
                        member.Zip = GetText(cell);

                        break;
                    case MemberListImportConfig.OurColumn.Town:
                        member.Town = GetText(cell);

                        break;
                    case MemberListImportConfig.OurColumn.Phone:
                        member.Phone = GetText(cell);

                        break;
                    case MemberListImportConfig.OurColumn.MemberFrom:
                        member.MemberFrom = GetDate(cell, header.Mapping.Format, DateOnly.MinValue);

                        break;
                    case MemberListImportConfig.OurColumn.MemberTo:
                        member.MemberFrom = GetDate(cell, header.Mapping.Format, DateOnly.MaxValue);

                        break;
                }
            }

            members.Add(member);
        }

        return members;
    }

    private static DateOnly GetDate(IXLCell cell, string? format, DateOnly fallback)
    {
        if (cell.Value.IsBlank)
        {
            return fallback;
        }

        if (cell.TryGetValue(out DateTime dateTime))
        {
            return DateOnly.FromDateTime(dateTime);
        }

        if (cell.TryGetValue(out string text))
        {
            return ParseDate(text, format, fallback);
        }

        return fallback;
    }

    private static string? GetText(IXLCell cell)
    {
        if (cell.TryGetValue(out string value))
        {
            return value.Trim();
        }

        if (cell.TryGetValue(out int number))
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }

        return null;
    }

    private static async Task<List<ImportedMember>> ParseCsv(ImportMemberListQuery query, MemberListImportConfig.ColumnMapping[] mappings)
    {
        using var reader = new StreamReader(query.File.FileStream);

        using var csv = new CsvReader(reader,
                                      new CsvConfiguration(CultureInfo.InvariantCulture)
                                      {
                                          HasHeaderRecord = true,
                                      });

        var headerMappings = mappings.ToDictionary(m => m.Header, m => m);

        var members = new List<ImportedMember>();
        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? [];

        while (await csv.ReadAsync())
        {
            var member = new ImportedMember
            {
                Id = Guid.NewGuid(),
                MemberFrom = DateOnly.MinValue,
                MemberUntil = DateOnly.MaxValue
            };

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

                    case MemberListImportConfig.OurColumn.Address:
                        member.Address = csv.GetField(header);

                        break;

                    case MemberListImportConfig.OurColumn.Zip:
                        member.Zip = csv.GetField(header);

                        break;
                    case MemberListImportConfig.OurColumn.Town:
                        member.Town = csv.GetField(header);

                        break;
                    case MemberListImportConfig.OurColumn.Phone:
                        member.Phone = csv.GetField(header);

                        break;
                    case MemberListImportConfig.OurColumn.MemberFrom:
                        member.MemberFrom = ParseDate(csv.GetField(header), mapping.Format, DateOnly.MinValue);

                        break;
                    case MemberListImportConfig.OurColumn.MemberTo:
                        member.MemberUntil = ParseDate(csv.GetField(header), mapping.Format, DateOnly.MaxValue);

                        break;
                }
            }

            members.Add(member);
        }

        return members;
    }

    private static DateOnly ParseDate(string? value, string? mappingFormat, DateOnly fallback)
    {
        if (value != null)
        {
            if (mappingFormat != null
                && DateOnly.TryParseExact(value, mappingFormat, out var date))
            {
                return date;
            }

            if (DateOnly.TryParse(value, out date))
            {
                return date;
            }
        }

        return fallback;
    }
}