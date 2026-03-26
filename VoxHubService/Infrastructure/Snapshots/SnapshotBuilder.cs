using VoxHubService.Infrastructure.Interfaces;
using VoxHubService.Infrastructure.Models.Chunk;
using VoxHubService.Infrastructure.Models.Snapshot;
using VoxHubService.Infrastructure.Models.Voxel;

namespace VoxHubService.Infrastructure.Snapshots;

public sealed class SnapshotBuilder
{
    private readonly IChunker _chunker;
    private readonly IChunkHasher _chunkHasher;

    public SnapshotBuilder(IChunker chunker, IChunkHasher chunkHasher)
    {
        _chunker = chunker ?? throw new ArgumentNullException(nameof(chunker));
        _chunkHasher = chunkHasher ?? throw new ArgumentNullException(nameof(chunkHasher));
    }

    public ModelSnapshot Build(VoxelModel model, Guid versionId)
    {
        ArgumentNullException.ThrowIfNull(model);

        var chunks = _chunker.BuildChunks(model);

        var chunkStates = chunks
            .Select(chunk => new ChunkState(
                chunk.Key,
                _chunkHasher.Hash(chunk)
            ))
            .OrderBy(x => x.Key.X)
            .ThenBy(x => x.Key.Y)
            .ThenBy(x => x.Key.Z)
            .ToList();

        return new ModelSnapshot(versionId, chunkStates);
    }
}