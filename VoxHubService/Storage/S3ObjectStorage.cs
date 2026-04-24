using Amazon.S3;
using VoxHubService.Interfaces;

namespace VoxHubService.Storage;

public sealed class S3ObjectStorage : IObjectStorage
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;

    public S3ObjectStorage(IAmazonS3 s3, string bucket)
    {
        _s3 = s3;
        _bucket = bucket;
    }

    public async Task PutAsync(string key, Stream content, System.Threading.CancellationToken ct = default)
    {
        await _s3.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = content
        }, ct);
    }

    public async Task<Stream> GetAsync(string key, System.Threading.CancellationToken ct = default)
    {
        var response = await _s3.GetObjectAsync(_bucket, key, ct);
        return response.ResponseStream;
    }

    public async Task<bool> ExistsAsync(string key, System.Threading.CancellationToken ct = default)
    {
        try
        {
            await _s3.GetObjectMetadataAsync(_bucket, key, ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task DeleteAsync(string key, System.Threading.CancellationToken ct = default)
    {
        await _s3.DeleteObjectAsync(_bucket, key, ct);
    }
}