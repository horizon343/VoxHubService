using VoxHubService.Domain;

namespace VoxHubService.Interfaces;

public interface IModelImporter
{
    Task<VoxelModel> ImportAsync(
        Stream input,
        CancellationToken cancellationToken = default
    );
}