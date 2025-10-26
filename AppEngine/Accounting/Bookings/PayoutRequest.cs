using AppEngine.DataAccess;
using AppEngine.Partitions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.Accounting.Bookings;

public class PayoutRequest : Entity
{
    public Guid PartitionId { get; set; }
    public Partition? Partition { get; set; }
    public string SourceType { get; set; }
    public Guid SourceId { get; set; }

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

        builder.Property(por => por.SourceType)
               .HasMaxLength(200);

        builder.Property(por => por.IbanProposed)
               .HasMaxLength(100);

        builder.Property(por => por.Reason)
               .HasMaxLength(500);
    }
}