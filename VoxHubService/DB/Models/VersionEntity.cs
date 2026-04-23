namespace VoxHubService.DB.Models;

public sealed class VersionEntity
{
    public Guid Id { get; set; }
    public Guid ModelId { get; set; }
    public Guid? ParentVersionId { get; set; }
    public VersionKind Kind { get; set; }
}