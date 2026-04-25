namespace VoxHubService.DB.Models;

public sealed class CommitEntity
{
    public Guid Id { get; set; }
    public Guid VersionId { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}