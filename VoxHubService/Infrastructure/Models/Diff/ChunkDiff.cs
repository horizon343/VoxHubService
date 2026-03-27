using VoxHubService.Infrastructure.Models.Chunk;

namespace VoxHubService.Infrastructure.Models.Diff;

public sealed record ChunkDiff(
    ChunkKey Key,
    ChunkChangeType Type
);