using VoxHubService.Infrastructure.Chunking;
using VoxHubService.Infrastructure.Diffing;
using VoxHubService.Infrastructure.Hashing;
using VoxHubService.Infrastructure.Interfaces;
using VoxHubService.Infrastructure.Mapping;
using VoxHubService.Infrastructure.Parsing;
using VoxHubService.Infrastructure.Snapshots;
using VoxHubService.Repositories;
using VoxHubService.Services;

namespace VoxHubService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddGrpc();

        // Readers
        builder.Services.AddSingleton<IVoxelFormatReader, VoxFormatReader>();

        // Core helpers
        builder.Services.AddSingleton<VoxDocumentMapper>();
        builder.Services.AddSingleton<IChunker>(_ => new FixedSizeChunker(16));
        builder.Services.AddSingleton<IChunkHasher, Sha256ChunkHasher>();
        builder.Services.AddSingleton<SnapshotBuilder>();
        builder.Services.AddSingleton<IDiffEngine, SnapshotDiffEngine>();
        builder.Services.AddSingleton<IVoxelLevelDiffEngine, VoxelLevelDiffEngine>();

        // Storage
        builder.Services.AddSingleton<IVersionRepository, InMemoryVersionRepository>();

        // App service
        builder.Services.AddScoped<IVersionService, VersionService>();

        var app = builder.Build();

        app.MapGrpcService<VersionApiService>();
        app.MapGet("/", () => "VoxelDiff gRPC service is running.");

        app.Run();
    }
}