using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using VoxHubService.DB;
using VoxHubService.Grpc;

namespace VoxHubService.Services;

public sealed class ModelQueryGrpcService : ModelQueryService.ModelQueryServiceBase
{
    private readonly VoxelDbContext _db;

    public ModelQueryGrpcService(VoxelDbContext db)
    {
        _db = db;
    }

    public override async Task<ListModelsResponse> ListModels(ListModelsRequest request, ServerCallContext context)
    {
        var models = await _db.Models
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