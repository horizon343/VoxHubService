namespace VoxHubService.Domain;

public readonly record struct Voxel(
    Int3 Position,
    byte PaletteIndex
);