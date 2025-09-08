using AppEngine.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.Accounting.Bookings;

public class BookingAssignment : Entity
{
    public Guid? SegmentId { get; set; }
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

        builder.HasOne(pas => pas.IncomingPayment)
               .WithMany(pmt => pmt.Assignments)
               .HasForeignKey(pas => pas.IncomingPaymentId);

        builder.HasOne(pas => pas.OutgoingPayment)
               .WithMany(pmt => pmt.Assignments)
               .HasForeignKey(pas => pas.OutgoingPaymentId);

        builder.HasOne(pas => pas.PayoutRequest)
               .WithMany(pmt => pmt.Assignments)
               .HasForeignKey(pas => pas.PayoutRequestId);

        builder.HasOne(pas => pas.PaymentAssignment_Counter)
               .WithMany()
               .HasForeignKey(pas => pas.PaymentAssignmentId_Counter);
    }
}