using VoxHubService.Infrastructure.Models.Snapshot;

namespace VoxHubService.Infrastructure.Interfaces;

public interface IVersionRepository
{
    Task SaveAsync(VersionData version, CancellationToken ct);
    Task<VersionData> GetAsync(Guid versionId, CancellationToken ct);
}