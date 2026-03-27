using Grpc.Core;
using VoxelDiff.Grpc;
using VoxHubService.Infrastructure.Interfaces;
using VoxHubService.Infrastructure.Models.Diff;
using VoxHubService.Infrastructure.Models.DiffDetail;

namespace VoxHubService.Services;

public sealed class VersionApiService : VersionApi.VersionApiBase
{
    private readonly IVersionService _versionService;

    public VersionApiService(IVersionService versionService)
    {
        _versionService = versionService ?? throw new ArgumentNullException(nameof(versionService));
    }

    public override async Task<UploadVoxResponse> UploadVox(UploadVoxRequest request, ServerCallContext context)
    {
        if (request.Data.IsEmpty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "File data is empty."));

        var formatId = GetFormatIdFromFileName(request.FileName);
        await using var stream = new MemoryStream(request.Data.ToByteArray());

        var versionId = await _versionService.ImportAsync(stream, formatId, context.CancellationToken);

        return new UploadVoxResponse
        {
            VersionId = versionId.ToString()
        };
    }

    public override async Task<CompareVersionsResponse> CompareVersions(
        CompareVersionsRequest request,
        ServerCallContext context)
    {
        var leftId = ParseGuid(request.LeftVersionId);
        var rightId = ParseGuid(request.RightVersionId);

        var diff = await _versionService.CompareAsync(leftId, rightId, context.CancellationToken);

        return new CompareVersionsResponse
        {
            Diff = Map(diff)
        };
    }

    public override async Task<CompareVersionsDetailedResponse> CompareVersionsDetailed(
        CompareVersionsRequest request,
        ServerCallContext context)
    {
        var leftId = ParseGuid(request.LeftVersionId);
        var rightId = ParseGuid(request.RightVersionId);

        var diff = await _versionService.CompareDetailedAsync(leftId, rightId, context.CancellationToken);

        return new CompareVersionsDetailedResponse
        {
            Diff = Map(diff)
        };
    }

    private static Guid ParseGuid(string value)
    {
        if (!Guid.TryParse(value, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid GUID: '{value}'."));

        return id;
    }

    private static string GetFormatIdFromFileName(string fileName)
    {
        var ext = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(ext))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "File name must have an extension."));

        return ext.TrimStart('.').ToLowerInvariant();
    }

    private static ModelDiffDto Map(ModelDiff diff)
    {
        return new ModelDiffDto
        {
            LeftVersionId = diff.LeftVersionId.ToString(),
            RightVersionId = diff.RightVersionId.ToString(),
            Chunks =
            {
                diff.Chunks.Select(x => new ChunkDiffDto
                {
                    X = x.Key.X,
                    Y = x.Key.Y,
                    Z = x.Key.Z,
                    ChangeType = x.Type.ToString()
                })
            }
        };
    }

    private static DetailedModelDiffDto Map(DetailedModelDiff diff)
    {
        return new DetailedModelDiffDto
        {
            LeftVersionId = diff.LeftVersionId.ToString(),
            RightVersionId = diff.RightVersionId.ToString(),
            Chunks =
            {
                diff.Chunks.Select(x => new VoxelChunkDiffDetailDto
                {
                    ChunkX = x.Key.X,
                    ChunkY = x.Key.Y,
                    ChunkZ = x.Key.Z,
                    ChunkType = x.ChunkType.ToString(),
                    Voxels =
                    {
                        x.Voxels.Select(v => new VoxelDeltaDto
                        {
                            ChangeType = v.Type.ToString(),
                            X = v.Right?.X ?? v.Left!.X,
                            Y = v.Right?.Y ?? v.Left!.Y,
                            Z = v.Right?.Z ?? v.Left!.Z,
                            ColorIndex = v.Right?.ColorIndex ?? v.Left!.ColorIndex
                        })
                    }
                })
            }
        };
    }
}