namespace VoxHubService.Interfaces;

public interface IObjectStorage
{
    Task PutAsync(string key, Stream content, CancellationToken cancellationToken = default);
    Task<Stream> GetAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}