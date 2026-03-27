using VoxHubService.Infrastructure.Models.Voxel;

namespace VoxHubService.Infrastructure.Models.Diff;

public sealed record VoxelDelta(
    VoxelChangeType Type,
    VoxelCell? Left,
    VoxelCell? Right
);