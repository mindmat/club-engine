using AppEngine.TimeHandling;

namespace ClubEngine.ApiService.Members.Import;

public record ImportedMember : IDatePeriod
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public Guid MembershipTypeId { get; set; }
    public Guid? CurrentMembershipTypeId { get; set; }
    public string? Address { get; set; }
    public string? Zip { get; set; }
    public string? Town { get; set; }
    public string? Phone { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateOnly From { get; set; }
    public DateOnly Until { get; set; }
    public string? PlannedLeave { get; set; }
}