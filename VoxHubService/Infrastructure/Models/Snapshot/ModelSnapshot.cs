using VoxHubService.Infrastructure.Models.Chunk;

namespace VoxHubService.Infrastructure.Models.Snapshot;

public sealed record ModelSnapshot(
    Guid VersionId,
    IReadOnlyList<ChunkState> Chunks
);