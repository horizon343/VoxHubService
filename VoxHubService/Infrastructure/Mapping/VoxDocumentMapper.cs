using VoxHubService.Infrastructure.Models.Vox;
using VoxHubService.Infrastructure.Models.Voxel;

namespace VoxHubService.Infrastructure.Mapping;

public sealed class VoxDocumentMapper
{
    public VoxelModel Map(VoxDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.Models.Count == 0)
            throw new InvalidDataException("The .vox document does not contain any models.");

        if (document.Models.Count > 1)
            throw new NotSupportedException(
                $"The .vox document contains {document.Models.Count} models. " +
                "For the first version of the pipeline only a single model is supported."
            );

        var model = document.Models[0];

        return new VoxelModel(
            Width: model.Width,
            Height: model.Height,
            Depth: model.Depth,
            Voxels: model.Voxels,
            PaletteArgb: document.PaletteArgb
        );
    }
}