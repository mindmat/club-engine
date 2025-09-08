using AppEngine.DataAccess;
using AppEngine.Partitions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.Accounting.Iso20022.Camt;

public class PaymentsFile : Entity
{
    public Guid? PartitionId { get; set; }
    public Partition? Partition { get; set; }

    public string? AccountIban { get; set; }
    public string? FileId { get; set; }
    public decimal? Balance { get; set; }
    public DateTime? BookingsFrom { get; set; }
    public DateTime? BookingsTo { get; set; }
    public string? Currency { get; set; }
    public string? Content { get; set; }
}

public class PaymentsFileMap : EntityMap<PaymentsFile>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PaymentsFile> builder)
    {
        builder.ToTable("PaymentsFiles");

        builder.HasOne(pmf => pmf.Partition)
               .WithMany()
               .HasForeignKey(pmf => pmf.PartitionId);

        builder.Property(pmf => pmf.AccountIban)
               .HasMaxLength(100);

        builder.Property(pmf => pmf.FileId)
               .HasMaxLength(100);

        builder.Property(pmf => pmf.Currency)
               .HasMaxLength(10);
    }
}