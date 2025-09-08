using AppEngine.DataAccess;
using AppEngine.Partitions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.Mailing.ExternalMails;

public class ExternalMail : Entity
{
    public Guid PartitionId { get; set; }
    public Partition? Partition { get; set; }
    public Guid? ExternalMailConfigurationId { get; set; }

    public string? SenderMail { get; set; }
    public string? SenderName { get; set; }
    public string? Subject { get; set; }
    public string? Recipients { get; set; }
    public string? ContentHtml { get; set; }
    public string? ContentPlainText { get; set; }
    public DateTimeOffset Date { get; set; }
    public DateTimeOffset Imported { get; set; }
    public string? MessageIdentifier { get; set; }
    public string? SendGridMessageId { get; set; }
}

public class ExternalMailMap : EntityMap<ExternalMail>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ExternalMail> builder)
    {
        builder.ToTable("ExternalMails");

        builder.HasOne(iml => iml.Partition)
               .WithMany()
               .HasForeignKey(iml => iml.PartitionId);

        builder.Property(iml => iml.SenderName)
               .HasMaxLength(200);

        builder.Property(iml => iml.SenderMail)
               .HasMaxLength(200);

        builder.Property(iml => iml.Recipients)
               .HasMaxLength(500);

        builder.Property(iml => iml.SendGridMessageId)
               .HasMaxLength(500);

        builder.Property(iml => iml.Subject)
               .HasMaxLength(500);

        builder.Property(iml => iml.MessageIdentifier)
               .HasMaxLength(500);
    }
}