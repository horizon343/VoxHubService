namespace VoxHubService.Infrastructure.Models.Diff;

public sealed record ModelDiff(
    Guid LeftVersionId,
    Guid RightVersionId,
    IReadOnlyList<ChunkDiff> Chunks
);