using System.Net;
using VoxHubService.Interfaces;

namespace VoxHubService.Storage;

public sealed class HttpObjectStorage : IObjectStorage
{
    private readonly HttpClient _httpClient;
    private readonly Uri _bucketUri;

    public HttpObjectStorage(HttpClient httpClient, Uri endpoint, string bucket)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _bucketUri = new Uri(endpoint ?? throw new ArgumentNullException(nameof(endpoint)), bucket.TrimEnd('/') + "/");
    }

    public async Task PutAsync(string key, Stream content, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, BuildObjectUri(key))
        {
            Content = new StreamContent(content)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Stream> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(BuildObjectUri(key), HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, BuildObjectUri(key));
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(BuildObjectUri(key), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return;

        response.EnsureSuccessStatusCode();
    }

    private Uri BuildObjectUri(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key is required.", nameof(key));

        var escapedKey = string.Join(
            "/",
            key.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(Uri.EscapeDataString));

        return new Uri(_bucketUri, escapedKey);
    }
}