namespace VoxHubService.Infrastructure.Models.DiffDetail;

public sealed record DetailedModelDiff(
    Guid LeftVersionId,
    Guid RightVersionId,
    IReadOnlyList<VoxelChunkDiffDetail> Chunks
);