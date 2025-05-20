using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.DataAccess;

public abstract class Entity
{
    public required Guid Id { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}

public abstract class EntityMap<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity
{
    private const string IncrementalKey = "IncrementalKey";

    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(ent => ent.Id)
               .IsClustered(false);

        ConfigureEntity(builder);

        var propertyBuilder = builder.Property<int>(IncrementalKey)
                                     .UseIdentityColumn();
        propertyBuilder.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        propertyBuilder.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasIndex(IncrementalKey)
               .IsUnique()
               .IsClustered();

        builder.Property(ent => ent.RowVersion)
               .IsRowVersion();
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}