namespace ClubEngine.ApiService.Members.Import;

public record ImportedMember
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Adresse { get; set; }
    public string? Plz { get; set; }
    public string? Ort { get; set; }
    public string? Mobile { get; set; }
    public List<string> Tags { get; set; } = new();
}