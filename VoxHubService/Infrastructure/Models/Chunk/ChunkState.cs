namespace VoxHubService.Infrastructure.Models.Chunk;

public sealed record ChunkState(
    ChunkKey Key,
    string Hash
);