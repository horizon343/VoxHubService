using System.Collections.Concurrent;
using VoxHubService.Infrastructure.Interfaces;
using VoxHubService.Infrastructure.Models.Snapshot;

namespace VoxHubService.Repositories;

public sealed class InMemoryVersionRepository : IVersionRepository
{
    private readonly ConcurrentDictionary<Guid, VersionData> _store = new();

    public Task SaveAsync(VersionData version, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(version);
        ct.ThrowIfCancellationRequested();

        _store[version.VersionId] = version;
        return Task.CompletedTask;
    }

    public Task<VersionData> GetAsync(Guid versionId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_store.TryGetValue(versionId, out var version))
            return Task.FromResult(version);

        throw new KeyNotFoundException($"Version '{versionId}' was not found.");
    }
}