using Microsoft.EntityFrameworkCore;
using VoxHubService.DB.Models;

namespace VoxHubService.DB;

public sealed class VoxelDbContext : DbContext
{
    public DbSet<ModelEntity> Models => Set<ModelEntity>();
    public DbSet<VersionEntity> Versions => Set<VersionEntity>();
    public DbSet<ChunkEntity> Chunks => Set<ChunkEntity>();
    public DbSet<CommitEntity> Commits => Set<CommitEntity>();
    public DbSet<SnapshotEntity> Snapshots => Set<SnapshotEntity>();
    public DbSet<DeltaPackEntity> DeltaPacks => Set<DeltaPackEntity>();

    public VoxelDbContext(DbContextOptions<VoxelDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ModelEntity>(e =>
        {
            e.ToTable("models");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
        });

        modelBuilder.Entity<VersionEntity>(e =>
        {
            e.ToTable("versions");
            e.HasKey(x => x.Id);

            e.Property(x => x.Kind).IsRequired();

            e.HasOne<ModelEntity>()
                .WithMany()
                .HasForeignKey(x => x.ModelId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<VersionEntity>()
                .WithMany()
                .HasForeignKey(x => x.ParentVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChunkEntity>(e =>
        {
            e.ToTable("chunks");
            e.HasKey(x => x.Id);

            e.HasOne<VersionEntity>()
                .WithMany()
                .HasForeignKey(x => x.VersionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.VersionId, x.ChunkX, x.ChunkY, x.ChunkZ }).IsUnique();
        });

        modelBuilder.Entity<CommitEntity>(e =>
        {
            e.ToTable("commits");
            e.HasKey(x => x.Id);

            e.HasOne<VersionEntity>()
                .WithOne()
                .HasForeignKey<CommitEntity>(x => x.VersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SnapshotEntity>(e =>
        {
            e.ToTable("snapshots");
            e.HasKey(x => x.Id);

            e.HasOne<VersionEntity>()
                .WithOne()
                .HasForeignKey<SnapshotEntity>(x => x.VersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeltaPackEntity>(e =>
        {
            e.ToTable("delta_packs");
            e.HasKey(x => x.Id);

            e.HasOne<VersionEntity>()
                .WithOne()
                .HasForeignKey<DeltaPackEntity>(x => x.VersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}