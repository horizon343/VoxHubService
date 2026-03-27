using VoxHubService.Infrastructure.Models.Diff;
using VoxHubService.Infrastructure.Models.DiffDetail;
using VoxHubService.Infrastructure.Models.Voxel;

namespace VoxHubService.Infrastructure.Interfaces;

public interface IVoxelLevelDiffEngine
{
    DetailedModelDiff Compare(VoxelModel left, VoxelModel right, ModelDiff chunkDiff);
}