using VoxHubService.Infrastructure.Models.Chunk;
using VoxHubService.Infrastructure.Models.Voxel;

namespace VoxHubService.Infrastructure.Interfaces;

public interface IChunker
{
    IReadOnlyList<VoxelChunk> BuildChunks(VoxelModel model);
}