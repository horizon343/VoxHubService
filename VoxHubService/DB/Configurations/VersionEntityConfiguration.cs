using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoxHubService.DB.Models;

namespace VoxHubService.DB.Configurations;

public sealed class VersionEntityConfiguration : IEntityTypeConfiguration<VersionEntity>
{
    public void Configure(EntityTypeBuilder<VersionEntity> e)
    {
        e.ToTable("versions");
        e.HasKey(x => x.Id);

        e.Property(x => x.ModelId).IsRequired();
        e.Property(x => x.Kind).IsRequired();

        e.HasOne<ModelEntity>()
            .WithMany()
            .HasForeignKey(x => x.ModelId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Commit)
            .WithOne()
            .HasForeignKey<CommitEntity>(x => x.VersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}