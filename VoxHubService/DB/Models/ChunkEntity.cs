namespace VoxHubService.DB.Models;

public sealed class ChunkEntity
{
    public Guid Id { get; set; }
    public Guid VersionId { get; set; }

    public int ChunkX { get; set; }
    public int ChunkY { get; set; }
    public int ChunkZ { get; set; }

    public string Hash { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
}