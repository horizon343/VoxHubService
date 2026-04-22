using VoxHubService.Domain.Canonical;

namespace VoxHubService.Domain.Serialization;

public static class CanonicalModelSerializer
{
    public static byte[] Serialize(VoxelModel model)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(model.SchemaVersion);
        WriteChunk(writer, model.RootChunk);

        writer.Flush();
        return stream.ToArray();
    }

    private static void WriteChunk(BinaryWriter writer, ChunkNode chunk)
    {
        writer.Write(chunk.Origin.X);
        writer.Write(chunk.Origin.Y);
        writer.Write(chunk.Origin.Z);

        writer.Write(chunk.Size.X);
        writer.Write(chunk.Size.Y);
        writer.Write(chunk.Size.Z);

        writer.Write(chunk.LodLevel);

        if (chunk.Children.Count > 0)
        {
            writer.Write((byte)1);
            writer.Write(chunk.Children.Count);

            foreach (var child in chunk.Children)
                WriteChunk(writer, child);

            return;
        }

        writer.Write((byte)0);
        writer.Write(chunk.Voxels.Count);

        foreach (var voxel in chunk.Voxels)
        {
            writer.Write(voxel.Position.X);
            writer.Write(voxel.Position.Y);
            writer.Write(voxel.Position.Z);
            writer.Write(voxel.PaletteIndex);
        }
    }
}