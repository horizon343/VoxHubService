using Grpc.Core;
using VoxHubService.Application;
using VoxHubService.Grpc;

namespace VoxHubService.Services;

public sealed class CommitImportGrpcService : CommitImportService.CommitImportServiceBase
{
    private readonly CommitImportPipeline _pipeline;

    public CommitImportGrpcService(CommitImportPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public override async Task<UploadCommitResponse> UploadCommit(
        IAsyncStreamReader<UploadCommitRequest> requestStream,
        ServerCallContext context)
    {
        if (!await requestStream.MoveNext(context.CancellationToken))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Empty upload stream."));

        var first = requestStream.Current;

        if (!Guid.TryParse(first.ModelId, out var modelId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "model_id is invalid."));

        if (!Guid.TryParse(first.ParentVersionId, out var parentVersionId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "parent_version_id is invalid."));

        if (string.IsNullOrWhiteSpace(first.Message))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "message is required."));

        if (first.ChunkSize <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "chunk_size must be > 0."));

        var tempPath = Path.GetTempFileName();

        try
        {
            await using (var temp = new FileStream(
                             tempPath,
                             FileMode.Create,
                             FileAccess.Write,
                             FileShare.None,
                             bufferSize: 81920,
                             useAsync: true))
            {
                if (first.Data.Length > 0)
                    await temp.WriteAsync(first.Data.ToByteArray(), context.CancellationToken);

                while (await requestStream.MoveNext(context.CancellationToken))
                {
                    var chunk = requestStream.Current;
                    if (chunk.Data.Length > 0)
                        await temp.WriteAsync(chunk.Data.ToByteArray(), context.CancellationToken);
                }
            }

            await using var input = File.OpenRead(tempPath);

            var versionId = await _pipeline.CommitAsync(
                modelId: modelId,
                parentVersionId: parentVersionId,
                voxStream: input,
                chunkSize: first.ChunkSize,
                message: first.Message,
                ct: context.CancellationToken);

            return new UploadCommitResponse
            {
                VersionId = versionId.ToString()
            };
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}