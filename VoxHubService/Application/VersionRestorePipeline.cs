using Microsoft.EntityFrameworkCore;
using VoxHubService.DB;
using VoxHubService.DB.Models;
using VoxHubService.Domain.Canonical;
using VoxHubService.Domain.Chunking;
using VoxHubService.Interfaces;

namespace VoxHubService.Application;

public sealed class VersionRestorePipeline
{
    private readonly IObjectStorage _storage;
    private readonly VoxelDbContext _db;

    public VersionRestorePipeline(IObjectStorage storage, VoxelDbContext db)
    {
        _storage = storage;
        _db = db;
    }

    public async Task<VoxelModel> RestoreAsync(Guid versionId, int chunkSize, CancellationToken ct = default)
    {
        var state = await LoadEffectiveStateAsync(versionId, chunkSize, ct);

        if (state.Count == 0)
        {
            return new VoxelModel
            {
                RootChunk = new ChunkNode
                {
                    Origin = new Int3(0, 0, 0),
                    Size = new Int3(0, 0, 0),
                    Children = Array.Empty<ChunkNode>(),
                    Voxels = Array.Empty<Voxel>()
                }
            };
        }

        var ordered = state.Values
            .OrderBy(x => x.Key.X)
            .ThenBy(x => x.Key.Y)
            .ThenBy(x => x.Key.Z)
            .ToArray();

        var minX = ordered.Min(x => x.Bounds.Min.X);
        var minY = ordered.Min(x => x.Bounds.Min.Y);
        var minZ = ordered.Min(x => x.Bounds.Min.Z);

        var maxX = ordered.Max(x => x.Bounds.Max.X);
        var maxY = ordered.Max(x => x.Bounds.Max.Y);
        var maxZ = ordered.Max(x => x.Bounds.Max.Z);

        return new VoxelModel
        {
            RootChunk = new ChunkNode
            {
                Origin = new Int3(minX, minY, minZ),
                Size = new Int3(maxX - minX, maxY - minY, maxZ - minZ),
                Children = ordered.Select(chunk => new ChunkNode
                {
                    Origin = chunk.Bounds.Min,
                    Size = new Int3(
                        chunk.Bounds.Max.X - chunk.Bounds.Min.X,
                        chunk.Bounds.Max.Y - chunk.Bounds.Min.Y,
                        chunk.Bounds.Max.Z - chunk.Bounds.Min.Z),
                    Voxels = chunk.Voxels,
                    Children = Array.Empty<ChunkNode>()
                }).ToArray(),
                Voxels = Array.Empty<Voxel>()
            }
        };
    }

    private async Task<Dictionary<ChunkKey, ChunkSlice>> LoadEffectiveStateAsync(Guid versionId, int chunkSize,
        CancellationToken ct)
    {
        var chain = new List<VersionEntity>();

        var current = await _db.Versions
            .AsNoTracking()
            .SingleAsync(x => x.Id == versionId, ct);

        while (true)
        {
            chain.Add(current);

            if (current.Kind == VersionKind.Snapshot || current.ParentVersionId is null)
                break;

            current = await _db.Versions
                .AsNoTracking()
                .SingleAsync(x => x.Id == current.ParentVersionId.Value, ct);
        }

        chain.Reverse();

        var state = new Dictionary<ChunkKey, ChunkSlice>();

        foreach (var version in chain)
        {
            var chunks = await _db.Chunks
                .AsNoTracking()
                .Where(x => x.VersionId == version.Id)
                .OrderBy(x => x.ChunkX)
                .ThenBy(x => x.ChunkY)
                .ThenBy(x => x.ChunkZ)
                .ToListAsync(ct);

            foreach (var chunk in chunks)
            {
                var key = new ChunkKey(chunk.ChunkX, chunk.ChunkY, chunk.ChunkZ);

                if (chunk.IsDeleted)
                {
                    state.Remove(key);
                    continue;
                }

                await using var blob = await _storage.GetAsync(chunk.ObjectKey, ct);
                var voxels = DeserializeChunk(blob);

                state[key] = new ChunkSlice(
                    key,
                    ChunkBounds.FromKey(key, chunkSize),
                    voxels,
                    chunk.Hash);
            }
        }

        return state;
    }

    private static IReadOnlyList<Voxel> DeserializeChunk(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        var count = reader.ReadInt32();
        var voxels = new Voxel[count];

        for (var i = 0; i < count; i++)
        {
            voxels[i] = new Voxel(
                new Int3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()),
                reader.ReadByte());
        }

        return voxels;
    }
}