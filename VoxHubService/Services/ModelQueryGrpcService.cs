using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using VoxHubService.DB;
using VoxHubService.Grpc;

namespace VoxHubService.Services;

public sealed class ModelQueryGrpcService(VoxelDbContext db) : ModelQueryService.ModelQueryServiceBase
{
    public override async Task<ListModelsResponse> ListModels(ListModelsRequest request, ServerCallContext context)
    {
        var models = await db.Models
            .AsNoTracking()
            .Select(x => new ModelDto
            {
                Id = x.Id.ToString(),
                Name = x.Name
            })
            .ToListAsync(context.CancellationToken);

        var response = new ListModelsResponse();
        response.Models.AddRange(models);

        return response;
    }
}