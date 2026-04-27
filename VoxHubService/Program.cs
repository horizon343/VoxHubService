using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using VoxHubService.Application;
using VoxHubService.DB;
using VoxHubService.Domain.Importing;
using VoxHubService.Interfaces;
using VoxHubService.Services;
using VoxHubService.Storage;

namespace VoxHubService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ===== CONFIG =====
        DotNetEnv.Env.Load();

        // ===== DB =====
        builder.Services.AddDbContext<VoxelDbContext>(options =>
            options.UseNpgsql(DotNetEnv.Env.GetString("VOXHUB_DB")));

        // ===== S3 =====
        builder.Services.AddSingleton<IAmazonS3>(_ =>
            new AmazonS3Client(
                DotNetEnv.Env.GetString("VOXHUB_S3_ACCESS_KEY"),
                DotNetEnv.Env.GetString("VOXHUB_S3_SECRET_KEY"),
                new AmazonS3Config
                {
                    ServiceURL = DotNetEnv.Env.GetString("VOXHUB_S3_ENDPOINT"),
                    ForcePathStyle = true,
                    AuthenticationRegion = DotNetEnv.Env.GetString("VOXHUB_S3_REGION")
                }));

        builder.Services.AddSingleton<IObjectStorage>(sp =>
        {
            var s3 = sp.GetRequiredService<IAmazonS3>();
            var bucket = DotNetEnv.Env.GetString("VOXHUB_S3_BUCKET");
            return new S3ObjectStorage(s3, bucket);
        });

        // ===== DOMAIN =====
        builder.Services.AddSingleton<IModelImporter, VoxModelImporter>();
        builder.Services.AddScoped<SnapshotImportPipeline>();
        builder.Services.AddScoped<CommitImportPipeline>();
        builder.Services.AddScoped<VersionRestorePipeline>();

        // ===== GRPC =====
        builder.Services.AddGrpc();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<VoxelDbContext>();
            db.Database.EnsureCreated();
        }

        app.MapGrpcService<SnapshotImportGrpcService>();
        app.MapGrpcService<CommitImportGrpcService>();
        app.MapGrpcService<ModelRestoreGrpcService>();
        app.MapGrpcService<ModelQueryGrpcService>();

        app.Run();
    }
}