using VoxHubService.Infrastructure.Models.Vox;

namespace VoxHubService.Infrastructure.Interfaces;

public interface IVoxelFormatReader
{
    string FormatId { get; }
    VoxDocument Read(Stream stream);
}