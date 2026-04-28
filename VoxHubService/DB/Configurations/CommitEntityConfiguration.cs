using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoxHubService.DB.Models;

namespace VoxHubService.DB.Configurations;

public sealed class CommitEntityConfiguration : IEntityTypeConfiguration<CommitEntity>
{
    public void Configure(EntityTypeBuilder<CommitEntity> e)
    {
        e.ToTable("commits");
        e.HasKey(x => x.Id);

        e.Property(x => x.Message)
            .HasMaxLength(2048)
            .IsRequired();
        e.Property(x => x.CreatedAtUtc).IsRequired();

        e.HasOne<VersionEntity>()
            .WithOne()
            .HasForeignKey<CommitEntity>(x => x.VersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}