using VoxHubService.Infrastructure.Models.Voxel;

namespace VoxHubService.Infrastructure.Models.Snapshot;

public sealed record VersionData(
    Guid VersionId,
    VoxelModel Model,
    ModelSnapshot Snapshot
);