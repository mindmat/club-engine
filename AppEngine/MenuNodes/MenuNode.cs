using AppEngine.DataAccess;
using AppEngine.Partitions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.MenuNodes;

public class MenuNodeReadModel : Entity
{
    public required Guid PartitionId { get; set; }
    public Partition? Partition { get; set; }
    public required string Key { get; set; }

    public string? Content { get; set; }
    public bool Hidden { get; set; }
    public MenuNodeStyle Style { get; set; }
}


public enum MenuNodeStyle
{
    None = 0,
    Info = 1,
    ToDo = 2,
    Important = 3
}

public class MenuNodeReadModelMap : EntityMap<MenuNodeReadModel>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MenuNodeReadModel> builder)
    {
        builder.ToTable("MenuNodeReadModels");

        builder.HasOne(mnd => mnd.Partition)
               .WithMany()
               .HasForeignKey(mnd => mnd.PartitionId);

        builder.Property(mnd => mnd.Key)
               .HasMaxLength(50);

        builder.Property(mnd => mnd.Content)
               .HasMaxLength(20);

        builder.HasIndex(mnd => new
                                {
                                    mnd.PartitionId,
                                    mnd.Key
                                })
               .IsUnique();
    }
}