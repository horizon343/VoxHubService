namespace VoxHubService.Infrastructure.Models.Vox;

public sealed record VoxDocument(
    int Version,
    IReadOnlyList<VoxModel> Models,
    IReadOnlyList<uint>? PaletteArgb
);