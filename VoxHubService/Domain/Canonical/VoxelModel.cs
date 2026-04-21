namespace VoxHubService.Domain.Canonical;

public sealed class VoxelModel
{
    public ushort SchemaVersion { get; init; } = 1;
    public required ChunkNode RootChunk { get; init; }
}