using VoxHubService.Domain.Chunking;

namespace VoxHubService.Domain.Versioning;

public sealed record ManifestChunk(
    ChunkKey Key,
    string Hash,
    string ObjectKey
);