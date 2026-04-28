using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using VoxHubService.DB;
using VoxHubService.Grpc;

namespace VoxHubService.Services;

public sealed class ModelQueryGrpcService(VoxelDbContext db) : ModelQueryService.ModelQueryServiceBase
{
    public override async Task<ListModelsResponse> ListModels(ListModelsRequest request, ServerCallContext context)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 100 : request.PageSize;

        var query = db.Models.AsNoTracking();

        var total = await query.CountAsync(context.CancellationToken);

        var models = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ModelDto
            {
                Id = x.Id.ToString(),
                Name = x.Name
            })
            .ToListAsync(context.CancellationToken);

        var response = new ListModelsResponse
        {
            Total = total
        };

        response.Models.AddRange(models);

        return response;
    }
}