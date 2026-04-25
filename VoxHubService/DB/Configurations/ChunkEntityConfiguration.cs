using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoxHubService.DB.Models;

namespace VoxHubService.DB.Configurations;

public sealed class ChunkEntityConfiguration : IEntityTypeConfiguration<ChunkEntity>
{
    public void Configure(EntityTypeBuilder<ChunkEntity> e)
    {
        e.ToTable("chunks");
        e.HasKey(x => x.Id);

        e.Property(x => x.IsDeleted).IsRequired();

        e.HasOne<VersionEntity>()
            .WithMany()
            .HasForeignKey(x => x.VersionId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasIndex(x => new { x.VersionId, x.ChunkX, x.ChunkY, x.ChunkZ })
            .IsUnique();
    }
}