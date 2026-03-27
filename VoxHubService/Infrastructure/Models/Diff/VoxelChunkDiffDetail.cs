using VoxHubService.Infrastructure.Models.Chunk;

namespace VoxHubService.Infrastructure.Models.Diff;

public sealed record VoxelChunkDiffDetail(
    ChunkKey Key,
    ChunkChangeType ChunkType,
    IReadOnlyList<VoxelDelta> Voxels
);