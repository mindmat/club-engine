using AppEngine.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.Accounting.Bookings;

public class PayoutRequest : Entity
{
    public Guid ExternalId { get; set; }

    public IList<BookingAssignment>? Assignments { get; set; }

    public decimal Amount { get; set; }
    public string? IbanProposed { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset Created { get; set; }
    public PayoutState State { get; set; }
}

public enum PayoutState
{
    Requested = 1,
    Sent      = 2,
    Confirmed = 3
}

public class PayoutRequestMap : EntityMap<PayoutRequest>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PayoutRequest> builder)
    {
        builder.ToTable("PayoutRequests");

        builder.Property(prq => prq.IbanProposed)
               .HasMaxLength(100);

        builder.Property(prq => prq.Reason)
               .HasMaxLength(500);
    }
}