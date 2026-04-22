using System.Security.Cryptography;
using VoxHubService.Domain.Canonical;

namespace VoxHubService.Domain.Chunking;

public static class ChunkHasher
{
    public static string Hash(ChunkKey key, ChunkBounds bounds, IReadOnlyList<Voxel> voxels)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(key.X);
        writer.Write(key.Y);
        writer.Write(key.Z);

        writer.Write(bounds.Min.X);
        writer.Write(bounds.Min.Y);
        writer.Write(bounds.Min.Z);

        writer.Write(bounds.Max.X);
        writer.Write(bounds.Max.Y);
        writer.Write(bounds.Max.Z);

        writer.Write(voxels.Count);

        foreach (var voxel in voxels)
        {
            writer.Write(voxel.Position.X);
            writer.Write(voxel.Position.Y);
            writer.Write(voxel.Position.Z);
            writer.Write(voxel.PaletteIndex);
        }

        writer.Flush();

        var hash = SHA256.HashData(stream.ToArray());
        return Convert.ToHexString(hash);
    }
}