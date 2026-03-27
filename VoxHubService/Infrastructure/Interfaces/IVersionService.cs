using VoxHubService.Infrastructure.Models.Diff;

namespace VoxHubService.Infrastructure.Interfaces;

public interface IVersionService
{
    Task<Guid> ImportAsync(Stream file, string formatId, CancellationToken ct);
    Task<ModelDiff> CompareAsync(Guid leftVersionId, Guid rightVersionId, CancellationToken ct);
    Task<DetailedModelDiff> CompareDetailedAsync(Guid leftVersionId, Guid rightVersionId, CancellationToken ct);
}