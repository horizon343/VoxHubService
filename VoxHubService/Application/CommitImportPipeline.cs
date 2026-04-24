using VoxHubService.DB;
using VoxHubService.Interfaces;
using Microsoft.EntityFrameworkCore;
using VoxHubService.DB.Models;
using VoxHubService.Domain.Chunking;

namespace VoxHubService.Application;

public sealed class CommitImportPipeline
{
    private readonly IModelImporter _importer;
    private readonly IObjectStorage _storage;
    private readonly VoxelDbContext _db;

    public CommitImportPipeline(IModelImporter importer, IObjectStorage storage, VoxelDbContext db)
    {
        _importer = importer;
        _storage = storage;
        _db = db;
    }

    public async Task<Guid> CommitAsync(
        Guid modelId,
        Guid parentVersionId,
        Stream voxStream,
        int chunkSize,
        string message,
        CancellationToken ct = default)
    {
        var parentVersion = await _db.Versions
            .AsNoTracking()
            .SingleAsync(x => x.Id == parentVersionId, ct);

        if (parentVersion.ModelId != modelId)
            throw new InvalidOperationException("Parent version does not belong to the model.");

        var model = await _importer.ImportAsync(voxStream, ct);
        var newChunks = FixedChunker.Split(model, chunkSize);

        var parentState = await LoadEffectiveStateAsync(parentVersionId, ct);
        var newState = newChunks.ToDictionary(x => x.Key);

        var changedChunks = new List<ChunkSlice>();
        foreach (var chunk in newChunks)
        {
            if (!parentState.TryGetValue(chunk.Key, out var parentChunk) || parentChunk.Hash != chunk.Hash)
                changedChunks.Add(chunk);
        }

        var deletedKeys = parentState.Keys
            .Where(key => !newState.ContainsKey(key))
            .ToArray();

        // No-op commit: identical state, no new version.
        if (changedChunks.Count == 0 && deletedKeys.Length == 0)
            return parentVersionId;

        var versionId = Guid.NewGuid();

        _db.Versions.Add(new VersionEntity
        {
            Id = versionId,
            ModelId = modelId,
            ParentVersionId = parentVersionId,
            Kind = VersionKind.Commit
        });

        _db.Commits.Add(new CommitEntity
        {
            Id = Guid.NewGuid(),
            VersionId = versionId,
            Message = message,
            CreatedAtUtc = DateTime.UtcNow
        });

        foreach (var chunk in changedChunks)
        {
            var objectKey = $"models/{modelId}/chunks/{chunk.Hash}.bin";

            await using (var ms = new MemoryStream(SerializeChunk(chunk)))
            {
                await _storage.PutAsync(objectKey, ms, ct);
            }

            _db.Chunks.Add(new ChunkEntity
            {
                Id = Guid.NewGuid(),
                VersionId = versionId,
                ChunkX = chunk.Key.X,
                ChunkY = chunk.Key.Y,
                ChunkZ = chunk.Key.Z,
                Hash = chunk.Hash,
                ObjectKey = objectKey,
                IsDeleted = false
            });
        }

        foreach (var key in deletedKeys)
        {
            _db.Chunks.Add(new ChunkEntity
            {
                Id = Guid.NewGuid(),
                VersionId = versionId,
                ChunkX = key.X,
                ChunkY = key.Y,
                ChunkZ = key.Z,
                Hash = string.Empty,
                ObjectKey = string.Empty,
                IsDeleted = true
            });
        }

        await _db.SaveChangesAsync(ct);
        return versionId;
    }

    private async Task<Dictionary<ChunkKey, ChunkEntity>> LoadEffectiveStateAsync(
        Guid versionId,
        CancellationToken ct)
    {
        var chain = new List<VersionEntity>();

        var current = await _db.Versions
            .AsNoTracking()
            .SingleAsync(x => x.Id == versionId, ct);

        while (true)
        {
            chain.Add(current);

            if (current.ParentVersionId is null)
                break;

            current = await _db.Versions
                .AsNoTracking()
                .SingleAsync(x => x.Id == current.ParentVersionId.Value, ct);
        }

        chain.Reverse();

        var state = new Dictionary<ChunkKey, ChunkEntity>();

        foreach (var version in chain)
        {
            var chunks = await _db.Chunks
                .AsNoTracking()
                .Where(x => x.VersionId == version.Id)
                .ToListAsync(ct);

            foreach (var chunk in chunks)
            {
                var key = new ChunkKey(chunk.ChunkX, chunk.ChunkY, chunk.ChunkZ);

                if (chunk.IsDeleted)
                    state.Remove(key);
                else
                    state[key] = chunk;
            }
        }

        return state;
    }

    private static byte[] SerializeChunk(ChunkSlice chunk)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(chunk.Voxels.Count);

        foreach (var v in chunk.Voxels)
        {
            writer.Write(v.Position.X);
            writer.Write(v.Position.Y);
            writer.Write(v.Position.Z);
            writer.Write(v.PaletteIndex);
        }

        writer.Flush();
        return stream.ToArray();
    }
}