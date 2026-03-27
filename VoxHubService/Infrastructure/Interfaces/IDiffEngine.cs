using VoxHubService.Infrastructure.Models.Diff;
using VoxHubService.Infrastructure.Models.Snapshot;

namespace VoxHubService.Infrastructure.Interfaces;

public interface IDiffEngine
{
    ModelDiff Compare(ModelSnapshot left, ModelSnapshot right);
}