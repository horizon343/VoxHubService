namespace VoxHubService.Domain.Canonical;

public sealed class ChunkNode
{
    public required Int3 Origin { get; init; }
    public required Int3 Size { get; init; }
    public IReadOnlyList<ChunkNode> Children { get; init; } = Array.Empty<ChunkNode>();
    public IReadOnlyList<Voxel> Voxels { get; init; } = Array.Empty<Voxel>();
}