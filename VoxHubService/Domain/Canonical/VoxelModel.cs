namespace VoxHubService.Domain.Canonical;

public sealed class VoxelModel
{
    public required ChunkNode RootChunk { get; init; }
}