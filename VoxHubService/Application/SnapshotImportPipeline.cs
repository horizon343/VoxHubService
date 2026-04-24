using VoxHubService.DB;
using VoxHubService.DB.Models;
using VoxHubService.Domain.Chunking;
using VoxHubService.Interfaces;

namespace VoxHubService.Application;

public sealed class SnapshotImportPipeline
{
    private readonly IModelImporter _importer;
    private readonly IObjectStorage _storage;
    private readonly VoxelDbContext _db;

    public SnapshotImportPipeline(IModelImporter importer, IObjectStorage storage, VoxelDbContext db)
    {
        _importer = importer;
        _storage = storage;
        _db = db;
    }

    public async Task<Guid> ImportAsync(string modelName, Stream voxStream, int chunkSize,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            throw new ArgumentException("Model name is required.", nameof(modelName));

        // 1. parse + canonical
        var model = await _importer.ImportAsync(voxStream, ct);
        Console.WriteLine("Import successful.");
        Console.WriteLine($"SchemaVersion: {model.SchemaVersion}");
        Console.WriteLine($"Root Size: {model.RootChunk.Size.X}, {model.RootChunk.Size.Y}, {model.RootChunk.Size.Z}");
        Console.WriteLine($"Voxels count: {model.RootChunk.Voxels.Count}");

        // 2. chunking + hashing
        var chunks = FixedChunker.Split(model, chunkSize);
        Console.WriteLine($"Chunks count: {chunks.Count}");

        var modelId = Guid.NewGuid();
        var versionId = Guid.NewGuid();

        // 3. db: model + version
        _db.Models.Add(new ModelEntity
        {
            Id = modelId,
            Name = modelName
        });

        _db.Versions.Add(new VersionEntity
        {
            Id = versionId,
            ModelId = modelId,
            Kind = VersionKind.Snapshot
        });

        // 4. upload chunks + save metadata
        foreach (var chunk in chunks)
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
                ObjectKey = objectKey
            });
        }

        // 5. snapshot metadata
        _db.Snapshots.Add(new SnapshotEntity
        {
            Id = Guid.NewGuid(),
            VersionId = versionId,
            ObjectKey = $"models/{modelId}/snapshot" // просто маркер snapshot
        });

        await _db.SaveChangesAsync(ct);
        Console.WriteLine("Saved to DB.");

        return versionId;
    }

    // Минимальная сериализация: только voxels
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