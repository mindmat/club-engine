using System.Globalization;

using AppEngine.Types;

using CsvHelper;
using CsvHelper.Configuration;

namespace ClubEngine.ApiService.Members.Import;

public class CsvMembersParser
{
    public static async Task<List<ImportedMember>> ParseCsv(ImportMemberListQuery query, MemberListImportConfig.ColumnMapping[] mappings)
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
                             From = DateOnly.MinValue,
                             Until = DateOnly.MaxValue
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
                        member.From = csv.GetField(header).ToDate(mapping.Format, DateOnly.MinValue);

                        break;
                    case MemberListImportConfig.OurColumn.MemberTo:
                        member.Until = csv.GetField(header).ToDate(mapping.Format, DateOnly.MaxValue);

                        break;
                }
            }

            members.Add(member);
        }

        return members;
    }
}