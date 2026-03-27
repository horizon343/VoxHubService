namespace VoxHubService.Infrastructure.Models.Diff;

public sealed record DetailedModelDiff(
    Guid LeftVersionId,
    Guid RightVersionId,
    IReadOnlyList<VoxelChunkDiffDetail> Chunks
);