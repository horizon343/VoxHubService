using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using VoxHubService.DB;
using VoxHubService.Grpc;

namespace VoxHubService.Services;

public sealed class VersionQueryGrpcService(VoxelDbContext db) : VersionQueryService.VersionQueryServiceBase
{
    public override async Task<ListVersionsResponse> ListVersions(ListVersionsRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.ModelId, out var modelId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid model_id"));

        var versions = await db.Versions
            .AsNoTracking()
            .Include(x => x.Commit)
            .Where(x => x.ModelId == modelId)
            .OrderBy(x => x.Id)
            .Select(x => new VersionDto
            {
                Id = x.Id.ToString(),
                ParentVersionId = x.ParentVersionId.HasValue
                    ? x.ParentVersionId.Value.ToString()
                    : string.Empty,
                Kind = x.Kind.ToString(),
                Commit = x.Commit == null
                    ? null
                    : new CommitDto
                    {
                        Id = x.Commit.Id.ToString(),
                        Message = x.Commit.Message,
                        CreatedAtUtc = new DateTimeOffset(x.Commit.CreatedAtUtc).ToUnixTimeMilliseconds()
                    }
            })
            .ToListAsync(context.CancellationToken);

        var response = new ListVersionsResponse();
        response.Versions.AddRange(versions);

        return response;
    }
}