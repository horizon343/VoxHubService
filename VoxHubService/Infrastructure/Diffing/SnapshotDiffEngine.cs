using VoxHubService.Infrastructure.Interfaces;
using VoxHubService.Infrastructure.Models.Diff;
using VoxHubService.Infrastructure.Models.Snapshot;

namespace VoxHubService.Infrastructure.Diffing;

public sealed class SnapshotDiffEngine : IDiffEngine
{
    public ModelDiff Compare(ModelSnapshot left, ModelSnapshot right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var leftMap = left.Chunks.ToDictionary(x => x.Key, x => x.Hash);
        var rightMap = right.Chunks.ToDictionary(x => x.Key, x => x.Hash);

        var allKeys = leftMap.Keys
            .Union(rightMap.Keys)
            .OrderBy(k => k.X)
            .ThenBy(k => k.Y)
            .ThenBy(k => k.Z);

        var diffs = new List<ChunkDiff>();

        foreach (var key in allKeys)
        {
            var inLeft = leftMap.TryGetValue(key, out var leftHash);
            var inRight = rightMap.TryGetValue(key, out var rightHash);

            ChunkChangeType type;

            if (inLeft && inRight)
            {
                type = leftHash == rightHash
                    ? ChunkChangeType.Unchanged
                    : ChunkChangeType.Modified;
            }
            else if (inRight)
            {
                type = ChunkChangeType.Added;
            }
            else
            {
                type = ChunkChangeType.Removed;
            }

            diffs.Add(new ChunkDiff(key, type));
        }

        return new ModelDiff(left.VersionId, right.VersionId, diffs);
    }
}