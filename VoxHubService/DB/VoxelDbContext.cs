using Microsoft.EntityFrameworkCore;
using VoxHubService.DB.Models;

namespace VoxHubService.DB;

public sealed class VoxelDbContext(DbContextOptions<VoxelDbContext> options) : DbContext(options)
{
    public DbSet<ModelEntity> Models => Set<ModelEntity>();
    public DbSet<VersionEntity> Versions => Set<VersionEntity>();
    public DbSet<ChunkEntity> Chunks => Set<ChunkEntity>();
    public DbSet<CommitEntity> Commits => Set<CommitEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VoxelDbContext).Assembly);
    }
}