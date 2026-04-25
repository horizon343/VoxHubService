using Grpc.Core;
using VoxHubService.Application;
using VoxHubService.Grpc;

namespace VoxHubService.Services;

public sealed class SnapshotImportGrpcService : SnapshotImportService.SnapshotImportServiceBase
{
    private readonly SnapshotImportPipeline _pipeline;

    public SnapshotImportGrpcService(SnapshotImportPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public override async Task<UploadSnapshotResponse> UploadSnapshot(
        IAsyncStreamReader<UploadSnapshotRequest> requestStream,
        ServerCallContext context)
    {
        if (!await requestStream.MoveNext(context.CancellationToken))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Empty upload stream."));

        var first = requestStream.Current;

        if (string.IsNullOrWhiteSpace(first.ModelName))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "model_name is required in first message."));

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

            var versionId = await _pipeline.ImportAsync(
                modelName: first.ModelName,
                voxStream: input,
                chunkSize: first.ChunkSize,
                ct: context.CancellationToken);

            return new UploadSnapshotResponse
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