using VoxHubService.Infrastructure.Interfaces;
using VoxHubService.Infrastructure.Mapping;
using VoxHubService.Infrastructure.Models.Diff;
using VoxHubService.Infrastructure.Models.Snapshot;
using VoxHubService.Infrastructure.Snapshots;

namespace VoxHubService.Services;

public sealed class VersionService : IVersionService
{
    private readonly IReadOnlyDictionary<string, IVoxelFormatReader> _readers;
    private readonly VoxDocumentMapper _voxMapper;
    private readonly SnapshotBuilder _snapshotBuilder;
    private readonly IDiffEngine _chunkDiffEngine;
    private readonly IVoxelLevelDiffEngine _voxelLevelDiffEngine;
    private readonly IVersionRepository _repository;

    public VersionService(
        IEnumerable<IVoxelFormatReader> readers,
        VoxDocumentMapper voxMapper,
        SnapshotBuilder snapshotBuilder,
        IDiffEngine chunkDiffEngine,
        IVoxelLevelDiffEngine voxelLevelDiffEngine,
        IVersionRepository repository)
    {
        _readers = readers?.ToDictionary(
            r => r.FormatId,
            StringComparer.OrdinalIgnoreCase
        ) ?? throw new ArgumentNullException(nameof(readers));

        _voxMapper = voxMapper ?? throw new ArgumentNullException(nameof(voxMapper));
        _snapshotBuilder = snapshotBuilder ?? throw new ArgumentNullException(nameof(snapshotBuilder));
        _chunkDiffEngine = chunkDiffEngine ?? throw new ArgumentNullException(nameof(chunkDiffEngine));
        _voxelLevelDiffEngine = voxelLevelDiffEngine ?? throw new ArgumentNullException(nameof(voxelLevelDiffEngine));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Guid> ImportAsync(Stream file, string formatId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);
        ct.ThrowIfCancellationRequested();

        if (!_readers.TryGetValue(formatId, out var reader))
            throw new NotSupportedException($"Format '{formatId}' is not registered.");

        // Пока поддерживаем канонизацию через VoxDocumentMapper.
        // Для новых форматов потом добавишь отдельный mapper или импорт-адаптер.
        var document = reader.Read(file);
        var model = _voxMapper.Map(document);

        var versionId = Guid.NewGuid();
        var snapshot = _snapshotBuilder.Build(model, versionId);

        var version = new VersionData(versionId, model, snapshot);
        await _repository.SaveAsync(version, ct);

        return versionId;
    }

    public async Task<ModelDiff> CompareAsync(Guid leftVersionId, Guid rightVersionId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var left = await _repository.GetAsync(leftVersionId, ct);
        var right = await _repository.GetAsync(rightVersionId, ct);

        return _chunkDiffEngine.Compare(left.Snapshot, right.Snapshot);
    }

    public async Task<DetailedModelDiff> CompareDetailedAsync(Guid leftVersionId, Guid rightVersionId,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var left = await _repository.GetAsync(leftVersionId, ct);
        var right = await _repository.GetAsync(rightVersionId, ct);

        var chunkDiff = _chunkDiffEngine.Compare(left.Snapshot, right.Snapshot);
        return _voxelLevelDiffEngine.Compare(left.Model, right.Model, chunkDiff);
    }
}