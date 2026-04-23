namespace VoxHubService.DB.Models;

public sealed class SnapshotEntity
{
    public Guid Id { get; set; }
    public Guid VersionId { get; set; }
    public string ObjectKey { get; set; } = string.Empty;
}