using VoxHubService.Infrastructure.Models.Voxel;

namespace VoxHubService.Infrastructure.Models.Vox;

public sealed record VoxModel(int Width, int Height, int Depth, IReadOnlyList<VoxelCell> Voxels);