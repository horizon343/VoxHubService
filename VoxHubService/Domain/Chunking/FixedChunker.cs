using VoxHubService.Domain.Canonical;

namespace VoxHubService.Domain.Chunking;

public static class FixedChunker
{
    public static IReadOnlyList<ChunkSlice> Split(VoxelModel model, int chunkSize)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));
        if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));

        var voxels = new List<Voxel>();
        CollectVoxels(model.RootChunk, voxels);

        var chunks = new Dictionary<ChunkKey, List<Voxel>>();

        foreach (var voxel in voxels)
        {
            var key = ChunkKey.FromPosition(voxel.Position, chunkSize);

            if (!chunks.TryGetValue(key, out var list))
            {
                list = new List<Voxel>();
                chunks.Add(key, list);
            }

            list.Add(voxel);
        }

        var keys = new List<ChunkKey>(chunks.Keys);
        keys.Sort(CompareKey);

        var result = new List<ChunkSlice>(keys.Count);

        foreach (var key in keys)
        {
            var chunkVoxels = chunks[key];
            chunkVoxels.Sort(CompareVoxel);

            var bounds = ChunkBounds.FromKey(key, chunkSize);
            var hash = ChunkHasher.Hash(key, bounds, chunkVoxels);

            result.Add(new ChunkSlice(key, bounds, chunkVoxels, hash));
        }

        return result;
    }

    private static void CollectVoxels(ChunkNode chunk, List<Voxel> output)
    {
        if (chunk.Children.Count > 0)
        {
            foreach (var child in chunk.Children)
                CollectVoxels(child, output);

            return;
        }

        output.AddRange(chunk.Voxels);
    }

    private static int CompareKey(ChunkKey a, ChunkKey b)
    {
        var c = a.X.CompareTo(b.X);
        if (c != 0) return c;

        c = a.Y.CompareTo(b.Y);
        if (c != 0) return c;

        return a.Z.CompareTo(b.Z);
    }

    private static int CompareVoxel(Voxel a, Voxel b)
    {
        var c = a.Position.X.CompareTo(b.Position.X);
        if (c != 0) return c;

        c = a.Position.Y.CompareTo(b.Position.Y);
        if (c != 0) return c;

        c = a.Position.Z.CompareTo(b.Position.Z);
        if (c != 0) return c;

        return a.PaletteIndex.CompareTo(b.PaletteIndex);
    }
}