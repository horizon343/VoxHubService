namespace VoxHubService.Domain.Versioning;

public sealed class VersionManifest
{
    public required Guid VersionId { get; init; }
    public Guid? BaseSnapshotVersionId { get; init; }
    public string? SnapshotObjectKey { get; init; }
    public string? DeltaObjectKey { get; init; }
    public required IReadOnlyList<ManifestChunk> Chunks { get; init; }
}