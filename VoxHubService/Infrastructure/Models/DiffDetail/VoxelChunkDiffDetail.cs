using VoxHubService.Infrastructure.Models.Chunk;
using VoxHubService.Infrastructure.Models.Diff;

namespace VoxHubService.Infrastructure.Models.DiffDetail;

public sealed record VoxelChunkDiffDetail(
    ChunkKey Key,
    ChunkChangeType ChunkType,
    IReadOnlyList<VoxelDelta> Voxels
);