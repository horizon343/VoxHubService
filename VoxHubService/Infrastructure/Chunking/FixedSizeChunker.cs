using VoxHubService.Infrastructure.Interfaces;
using VoxHubService.Infrastructure.Models.Chunk;
using VoxHubService.Infrastructure.Models.Voxel;

namespace VoxHubService.Infrastructure.Chunking;

public sealed class FixedSizeChunker : IChunker
{
    private readonly int _chunkSize;

    public FixedSizeChunker(int chunkSize = 16)
    {
        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be positive.");

        _chunkSize = chunkSize;
    }

    public IReadOnlyList<VoxelChunk> BuildChunks(VoxelModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var buckets = new Dictionary<ChunkKey, List<VoxelCell>>();

        foreach (var voxel in model.Voxels)
        {
            ValidateVoxelInsideModel(model, voxel);

            var key = new ChunkKey(
                voxel.X / _chunkSize,
                voxel.Y / _chunkSize,
                voxel.Z / _chunkSize
            );

            if (!buckets.TryGetValue(key, out var list))
            {
                list = new List<VoxelCell>();
                buckets[key] = list;
            }

            list.Add(voxel);
        }

        return buckets
            .OrderBy(k => k.Key.X)
            .ThenBy(k => k.Key.Y)
            .ThenBy(k => k.Key.Z)
            .Select(kvp => new VoxelChunk(kvp.Key, kvp.Value))
            .ToList();
    }

    private static void ValidateVoxelInsideModel(VoxelModel model, VoxelCell voxel)
    {
        if (voxel.X < 0 || voxel.X >= model.Width ||
            voxel.Y < 0 || voxel.Y >= model.Height ||
            voxel.Z < 0 || voxel.Z >= model.Depth)
        {
            throw new InvalidDataException(
                $"Voxel ({voxel.X}, {voxel.Y}, {voxel.Z}) is outside model bounds " +
                $"({model.Width}, {model.Height}, {model.Depth})."
            );
        }
    }
}