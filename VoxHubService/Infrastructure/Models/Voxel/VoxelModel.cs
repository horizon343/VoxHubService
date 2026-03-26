namespace VoxHubService.Infrastructure.Models.Voxel;

public sealed record VoxelModel(
    int Width,
    int Height,
    int Depth,
    IReadOnlyList<VoxelCell> Voxels,
    IReadOnlyList<uint>? PaletteArgb = null
);