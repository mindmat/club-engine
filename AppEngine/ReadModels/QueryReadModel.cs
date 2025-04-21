using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.ReadModels;

public class QueryReadModel
{
    public string QueryName { get; set; } = null!;
    public Guid PartitionId { get; set; }
    public Guid? RowId { get; set; }
    public string ContentJson { get; set; } = null!;
    public DateTimeOffset LastUpdate { get; set; }
}

public class QueryReadModelMap : IEntityTypeConfiguration<QueryReadModel>
{
    private const string Sequence = "Sequence";

    public void Configure(EntityTypeBuilder<QueryReadModel> builder)
    {
        builder.ToTable("ReadModels");

        var propertyBuilder = builder.Property<int>(Sequence)
                                     .UseIdentityColumn();
        propertyBuilder.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        propertyBuilder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasKey(Sequence)
               .IsClustered();

        builder.HasIndex(rdm => new
                                {
                                    rdm.QueryName,
                                    rdm.PartitionId,
                                    rdm.RowId
                                })
               .IsUnique();
    }
}