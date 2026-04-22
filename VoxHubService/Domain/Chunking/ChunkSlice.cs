using VoxHubService.Domain.Canonical;

namespace VoxHubService.Domain.Chunking;

public sealed record ChunkSlice(
    ChunkKey Key,
    ChunkBounds Bounds,
    IReadOnlyList<Voxel> Voxels,
    string Hash
);