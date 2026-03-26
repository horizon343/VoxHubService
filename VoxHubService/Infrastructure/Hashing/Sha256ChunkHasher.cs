using System.Security.Cryptography;
using System.Text;
using VoxHubService.Infrastructure.Interfaces;
using VoxHubService.Infrastructure.Models.Chunk;

namespace VoxHubService.Infrastructure.Hashing;

public sealed class Sha256ChunkHasher : IChunkHasher
{
    public string Hash(VoxelChunk chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        // Сортируем, чтобы hash не зависел от порядка в списке voxels.
        var orderedVoxels = chunk.Voxels
            .OrderBy(v => v.X)
            .ThenBy(v => v.Y)
            .ThenBy(v => v.Z)
            .ThenBy(v => v.ColorIndex);

        var sb = new StringBuilder();

        foreach (var voxel in orderedVoxels)
        {
            sb.Append(voxel.X).Append(',')
                .Append(voxel.Y).Append(',')
                .Append(voxel.Z).Append(',')
                .Append(voxel.ColorIndex).Append(';');
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var hashBytes = SHA256.HashData(bytes);

        return Convert.ToHexString(hashBytes);
    }
}