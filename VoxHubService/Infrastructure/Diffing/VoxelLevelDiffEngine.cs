using VoxHubService.Infrastructure.Interfaces;
using VoxHubService.Infrastructure.Models.Chunk;
using VoxHubService.Infrastructure.Models.Diff;
using VoxHubService.Infrastructure.Models.DiffDetail;
using VoxHubService.Infrastructure.Models.Voxel;

namespace VoxHubService.Infrastructure.Diffing;

public sealed class VoxelLevelDiffEngine : IVoxelLevelDiffEngine
{
    private readonly IChunker _chunker;

    public VoxelLevelDiffEngine(IChunker chunker)
    {
        _chunker = chunker ?? throw new ArgumentNullException(nameof(chunker));
    }

    public DetailedModelDiff Compare(VoxelModel left, VoxelModel right, ModelDiff chunkDiff)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(chunkDiff);

        var leftChunks = _chunker.BuildChunks(left).ToDictionary(x => x.Key, x => x);
        var rightChunks = _chunker.BuildChunks(right).ToDictionary(x => x.Key, x => x);

        var details = new List<VoxelChunkDiffDetail>();

        foreach (var chunk in chunkDiff.Chunks.Where(x => x.Type != ChunkChangeType.Unchanged))
        {
            leftChunks.TryGetValue(chunk.Key, out var leftChunk);
            rightChunks.TryGetValue(chunk.Key, out var rightChunk);

            var detail = CompareChunk(chunk.Key, chunk.Type, leftChunk, rightChunk);
            details.Add(detail);
        }

        return new DetailedModelDiff(
            chunkDiff.LeftVersionId,
            chunkDiff.RightVersionId,
            details
        );
    }

    private static VoxelChunkDiffDetail CompareChunk(
        ChunkKey key,
        ChunkChangeType chunkType,
        VoxelChunk? leftChunk,
        VoxelChunk? rightChunk)
    {
        if (chunkType == ChunkChangeType.Added)
        {
            if (rightChunk is null)
                throw new InvalidDataException($"Chunk {key} is marked as Added, but right side is missing.");

            var voxels = rightChunk.Voxels
                .Select(v => new VoxelDelta(VoxelChangeType.Added, null, v))
                .ToList();

            return new VoxelChunkDiffDetail(key, chunkType, voxels);
        }

        if (chunkType == ChunkChangeType.Removed)
        {
            if (leftChunk is null)
                throw new InvalidDataException($"Chunk {key} is marked as Removed, but left side is missing.");

            var voxels = leftChunk.Voxels
                .Select(v => new VoxelDelta(VoxelChangeType.Removed, v, null))
                .ToList();

            return new VoxelChunkDiffDetail(key, chunkType, voxels);
        }

        if (leftChunk is null || rightChunk is null)
            throw new InvalidDataException($"Chunk {key} is marked as Modified, but one side is missing.");

        var leftMap = leftChunk.Voxels.ToDictionary(v => (v.X, v.Y, v.Z));
        var rightMap = rightChunk.Voxels.ToDictionary(v => (v.X, v.Y, v.Z));

        var allPositions = leftMap.Keys
            .Union(rightMap.Keys)
            .OrderBy(p => p.Item1)
            .ThenBy(p => p.Item2)
            .ThenBy(p => p.Item3);

        var deltas = new List<VoxelDelta>();

        foreach (var pos in allPositions)
        {
            var inLeft = leftMap.TryGetValue(pos, out var leftVoxel);
            var inRight = rightMap.TryGetValue(pos, out var rightVoxel);

            if (inLeft && inRight)
            {
                if (leftVoxel!.ColorIndex != rightVoxel!.ColorIndex)
                {
                    deltas.Add(new VoxelDelta(VoxelChangeType.Modified, leftVoxel, rightVoxel));
                }
            }
            else if (inRight)
            {
                deltas.Add(new VoxelDelta(VoxelChangeType.Added, null, rightVoxel));
            }
            else
            {
                deltas.Add(new VoxelDelta(VoxelChangeType.Removed, leftVoxel, null));
            }
        }

        return new VoxelChunkDiffDetail(key, chunkType, deltas);
    }
}