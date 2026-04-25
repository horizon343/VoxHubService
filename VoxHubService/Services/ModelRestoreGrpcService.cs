using Google.Protobuf;
using Grpc.Core;
using VoxHubService.Application;
using VoxHubService.Domain.Exporting;
using VoxHubService.Grpc;

namespace VoxHubService.Services;

public sealed class ModelRestoreGrpcService : ModelRestoreService.ModelRestoreServiceBase
{
    private readonly VersionRestorePipeline _pipeline;

    public ModelRestoreGrpcService(VersionRestorePipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public override async Task DownloadModel(
        DownloadModelRequest request,
        IServerStreamWriter<DownloadModelResponse> responseStream,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.VersionId, out var versionId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid version_id"));

        if (request.ChunkSize <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "chunk_size must be > 0"));

        // restore model
        var model = await _pipeline.RestoreAsync(
            versionId,
            request.ChunkSize,
            context.CancellationToken);

        // export to memory stream (минимальный MVP)
        await using var ms = new MemoryStream();
        VoxModelExporter.Export(model, ms);

        ms.Position = 0;

        const int bufferSize = 64 * 1024;
        var buffer = new byte[bufferSize];

        while (true)
        {
            var read = await ms.ReadAsync(buffer, 0, buffer.Length, context.CancellationToken);
            if (read == 0)
                break;

            await responseStream.WriteAsync(new DownloadModelResponse
            {
                Data = ByteString.CopyFrom(buffer, 0, read)
            });
        }
    }
}