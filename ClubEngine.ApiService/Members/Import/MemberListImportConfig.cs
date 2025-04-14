using AppEngine.Configurations;

namespace ClubEngine.ApiService.Members.Import;

public class MemberListImportConfig : IConfigurationItem
{
    public Source[] Sources { get; set; } = [];

    public record Source(string Name, FileType FileType, ColumnMapping[] Mappings);

    public record ColumnMapping(string Header, OurColumn OurColumn, string? Tag = null, string? TagActive = null);

    public enum FileType
    {
        Csv = 1
    }

    public enum OurColumn
    {
        FirstName = 1,
        LastName = 2,
        Email = 3,
        Tag = 4,
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
                       ])
        ];
    }
}