using System.Globalization;

using AppEngine.Types;

using ClosedXML.Excel;

namespace ClubEngine.ApiService.Members.Import;

public class XlsxMembersParser
{
    public static List<ImportedMember> ParseXlsx(ImportMemberListQuery query, MemberListImportConfig.ColumnMapping[] mappings)
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

                    case MemberListImportConfig.OurColumn.MembershipType:
                        member.MembershipTypeId = GetGuid(cell.Value, header.Mapping.Mappings, header.Mapping.FallbackValue);

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
                        member.MemberUntil = GetDate(cell, header.Mapping.Format, DateOnly.MaxValue);

                        break;
                }
            }

            members.Add(member);
        }

        return members;
    }

    private static Guid GetGuid(XLCellValue value, IDictionary<string, object>? mappings, object? fallbackValue)
    {
        if (value.IsText)
        {
            var text = value.GetText();

            if (mappings?.TryGetValue(text.ToLowerInvariant(), out var lookup) == true)
            {
                return Guid.Parse(lookup.ToString()!);
            }
        }

        return Guid.Parse(fallbackValue!.ToString()!);
    }

    private static TEnum? GetEnum<TEnum>(XLCellValue value, IDictionary<string, object>? mappings, object? fallbackValue)
    {
        if (value.IsText)
        {
            var text = value.GetText();

            if (mappings?.TryGetValue(text.ToLowerInvariant(), out var lookup) == true)
            {
                return (TEnum)lookup;
            }
        }

        return (TEnum?)fallbackValue;
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
            return text.ToDate(format, fallback);
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
}