using AppEngine.Configurations;

namespace ClubEngine.ApiService.Members.Import;

public class MemberListImportConfig : IConfigurationItem
{
    public Source[] Sources { get; set; } = [];

    public record Source(string Name, FileType FileType, ColumnMapping[] Mappings);

    public record ColumnMapping(string Header,
                                OurColumn OurColumn,
                                string? Tag = null,
                                string? TagActive = null,
                                string? Format = null,
                                object? FallbackValue = null,
                                IDictionary<string, object>? Mappings = null);

    public enum FileType
    {
        Unknown = 0,
        Csv     = 1,
        Xlsx    = 2
    }

    public enum OurColumn
    {
        FirstName      = 1,
        LastName       = 2,
        Email          = 3,
        MembershipType = 4,
        Tag            = 5,
        Address        = 6,
        Zip            = 7,
        Town           = 8,
        Phone          = 9,
        MemberFrom     = 10,
        MemberTo       = 11
    }
}

public class MemberListImportDefaultConfigProvider : MemberListImportConfig, IDefaultConfigurationItem
{
    public MemberListImportDefaultConfigProvider()
    {
        Sources =
        [
            new Source("Webseite",
                       FileType.Csv,
                       [
                           new ColumnMapping("Vorname", OurColumn.FirstName),
                           new ColumnMapping("Name", OurColumn.LastName),
                           new ColumnMapping("Email", OurColumn.Email),
                           new ColumnMapping("Admin", OurColumn.Tag, "Admin", "Active"),
                           new ColumnMapping("Teacher", OurColumn.Tag, "Teacher", "Active"),
                           new ColumnMapping("Veranstalter", OurColumn.Tag, "Veranstalter", "Active"),
                       ]),
            new Source("OneDrive",
                       FileType.Xlsx,
                       [
                           new ColumnMapping("Vorname", OurColumn.FirstName),
                           new ColumnMapping("Name", OurColumn.LastName),
                           new ColumnMapping("Email", OurColumn.Email),
                           new ColumnMapping("Mitgliedschafts-Typ",
                                             OurColumn.MembershipType,
                                             FallbackValue: MembershipType.Active,
                                             Mappings: new Dictionary<string, object> { { "passiv", MembershipType.Passive }, { "ehrenmitglied", MembershipType.Honorary } }),
                           new ColumnMapping("Adresse", OurColumn.Address),
                           new ColumnMapping("PLZ", OurColumn.Zip),
                           new ColumnMapping("Ort", OurColumn.Town),
                           new ColumnMapping("Mobile", OurColumn.Phone),
                           new ColumnMapping("Eintritt", OurColumn.MemberFrom, Format: "yyyy-MM"),
                           new ColumnMapping("Austritt", OurColumn.MemberTo, Format: "yyyy-MM"),
                       ]),
        ];
    }
}