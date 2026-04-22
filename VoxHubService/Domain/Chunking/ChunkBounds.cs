using VoxHubService.Domain.Canonical;

namespace VoxHubService.Domain.Chunking;

public readonly record struct ChunkBounds(Int3 Min, Int3 Max)
{
    public static ChunkBounds FromKey(ChunkKey key, int chunkSize)
    {
        var min = key.ToOrigin(chunkSize);
        return new ChunkBounds(
            min,
            new Int3(min.X + chunkSize, min.Y + chunkSize, min.Z + chunkSize)
        );
    }

    public bool Contains(Int3 position) =>
        position.X >= Min.X && position.X < Max.X &&
        position.Y >= Min.Y && position.Y < Max.Y &&
        position.Z >= Min.Z && position.Z < Max.Z;
}