using VoxHubService.Domain.Canonical;

namespace VoxHubService.Domain.Chunking;

public readonly record struct ChunkKey(int X, int Y, int Z)
{
    public Int3 ToOrigin(int chunkSize) => new(X * chunkSize, Y * chunkSize, Z * chunkSize);

    public static ChunkKey FromPosition(Int3 position, int chunkSize) =>
        new(
            FloorDiv(position.X, chunkSize),
            FloorDiv(position.Y, chunkSize),
            FloorDiv(position.Z, chunkSize)
        );

    private static int FloorDiv(int value, int divisor)
    {
        var quotient = value / divisor;
        var remainder = value % divisor;

        if (remainder < 0)
            quotient--;

        return quotient;
    }
}