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

    public override async Task<UploadSnapshotResponse> UploadSnapshot(UploadSnapshotRequest request, ServerCallContext context)
    {
        await using var stream = new MemoryStream(request.VoxData.ToByteArray());

        var versionId = await _pipeline.ImportAsync(
            modelName: request.ModelName,
            voxStream: stream,
            chunkSize: request.ChunkSize,
            ct: context.CancellationToken);

        return new UploadSnapshotResponse
        {
            VersionId = versionId.ToString()
        };
    }
}