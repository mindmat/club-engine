namespace ClubEngine.ApiService.Members.Import;

public record ImportedMember
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Zip { get; set; }
    public string? Town { get; set; }
    public string? Phone { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateOnly MemberFrom { get; set; }
    public DateOnly MemberUntil { get; set; }
}