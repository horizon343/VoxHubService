using VoxHubService.Infrastructure.Models.Chunk;

namespace VoxHubService.Infrastructure.Interfaces;

public interface IChunkHasher
{
    string Hash(VoxelChunk chunk);
}