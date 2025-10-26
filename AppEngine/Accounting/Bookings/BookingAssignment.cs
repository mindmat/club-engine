using AppEngine.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.Accounting.Bookings;

public class BookingAssignment : Entity
{
    public Guid? SourceId { get; set; }
    public string? SourceType { get; set; }
    public Guid? IncomingPaymentId { get; set; }
    public IncomingPayment? IncomingPayment { get; set; }
    public Guid? OutgoingPaymentId { get; set; }
    public OutgoingPayment? OutgoingPayment { get; set; }
    public Guid? PaymentAssignmentId_Counter { get; set; }
    public BookingAssignment? PaymentAssignment_Counter { get; set; }
    public Guid? PayoutRequestId { get; set; }
    public PayoutRequest? PayoutRequest { get; set; }

    public decimal Amount { get; set; }
    public DateTimeOffset? Created { get; set; }
}

public class PaymentAssignmentMap : EntityMap<BookingAssignment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BookingAssignment> builder)
    {
        builder.ToTable("PaymentAssignments");

        builder.Property(bas => bas.SourceType)
               .HasMaxLength(200);

        builder.HasOne(bas => bas.IncomingPayment)
               .WithMany(ipm => ipm.Assignments)
               .HasForeignKey(bas => bas.IncomingPaymentId);

        builder.HasOne(bas => bas.OutgoingPayment)
               .WithMany(opm => opm.Assignments)
               .HasForeignKey(bas => bas.OutgoingPaymentId);

        builder.HasOne(bas => bas.PayoutRequest)
               .WithMany(por => por.Assignments)
               .HasForeignKey(bas => bas.PayoutRequestId);

        builder.HasOne(bas => bas.PaymentAssignment_Counter)
               .WithMany()
               .HasForeignKey(bas => bas.PaymentAssignmentId_Counter);
    }
}