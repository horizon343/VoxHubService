using VoxHubService.Infrastructure.Models.Voxel;

namespace VoxHubService.Infrastructure.Models.Chunk;

public sealed record VoxelChunk(
    ChunkKey Key,
    IReadOnlyList<VoxelCell> Voxels
);