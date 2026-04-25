using System.Text;
using VoxHubService.Domain.Canonical;

namespace VoxHubService.Domain.Exporting;

public static class VoxModelExporter
{
    public static void Export(VoxelModel model, Stream output)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));
        if (output is null) throw new ArgumentNullException(nameof(output));

        var voxels = CollectVoxels(model.RootChunk)
            .OrderBy(v => v.Position.X)
            .ThenBy(v => v.Position.Y)
            .ThenBy(v => v.Position.Z)
            .ThenBy(v => v.PaletteIndex)
            .ToArray();

        var (minX, minY, minZ, maxX, maxY, maxZ) = GetBounds(voxels);

        var sizeX = maxX - minX + 1;
        var sizeY = maxY - minY + 1;
        var sizeZ = maxZ - minZ + 1;

        if (sizeX > byte.MaxValue || sizeY > byte.MaxValue || sizeZ > byte.MaxValue)
            throw new NotSupportedException("Model is too large for a single .vox file in this minimal exporter.");

        using var writer = new BinaryWriter(output, Encoding.ASCII, leaveOpen: true);

        // Header: "VOX " + version 150
        writer.Write(Encoding.ASCII.GetBytes("VOX "));
        writer.Write(150);

        // Build MAIN children in memory to know their exact byte size.
        using var childrenStream = new MemoryStream();
        using (var childrenWriter = new BinaryWriter(childrenStream, Encoding.ASCII, leaveOpen: true))
        {
            WriteChunk(childrenWriter, "SIZE", w =>
            {
                w.Write(sizeX);
                w.Write(sizeY);
                w.Write(sizeZ);
            });

            WriteChunk(childrenWriter, "XYZI", w =>
            {
                w.Write(voxels.Length);

                foreach (var voxel in voxels)
                {
                    w.Write((byte)(voxel.Position.X - minX));
                    w.Write((byte)(voxel.Position.Y - minY));
                    w.Write((byte)(voxel.Position.Z - minZ));
                    w.Write(voxel.PaletteIndex);
                }
            });

            childrenWriter.Flush();
        }

        // MAIN chunk: empty content, children = SIZE + XYZI.
        writer.Write(Encoding.ASCII.GetBytes("MAIN"));
        writer.Write(0); // content bytes
        writer.Write((int)childrenStream.Length);
        writer.Write(childrenStream.ToArray());
        writer.Flush();
    }

    private static void WriteChunk(BinaryWriter writer, string id, Action<BinaryWriter> writeContent)
    {
        using var contentStream = new MemoryStream();
        using (var contentWriter = new BinaryWriter(contentStream, Encoding.ASCII, leaveOpen: true))
        {
            writeContent(contentWriter);
            contentWriter.Flush();
        }

        writer.Write(Encoding.ASCII.GetBytes(id));
        writer.Write((int)contentStream.Length);
        writer.Write(0); // no child chunks in this minimal exporter
        writer.Write(contentStream.ToArray());
    }

    private static IReadOnlyList<Voxel> CollectVoxels(ChunkNode chunk)
    {
        var result = new List<Voxel>();
        Collect(chunk, result);
        return result;

        static void Collect(ChunkNode node, List<Voxel> list)
        {
            if (node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                    Collect(child, list);

                return;
            }

            list.AddRange(node.Voxels);
        }
    }

    private static (int minX, int minY, int minZ, int maxX, int maxY, int maxZ) GetBounds(IReadOnlyList<Voxel> voxels)
    {
        if (voxels.Count == 0)
            return (0, 0, 0, 0, 0, 0);

        var minX = voxels[0].Position.X;
        var minY = voxels[0].Position.Y;
        var minZ = voxels[0].Position.Z;

        var maxX = minX;
        var maxY = minY;
        var maxZ = minZ;

        for (var i = 1; i < voxels.Count; i++)
        {
            var p = voxels[i].Position;

            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.Z < minZ) minZ = p.Z;

            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
            if (p.Z > maxZ) maxZ = p.Z;
        }

        return (minX, minY, minZ, maxX, maxY, maxZ);
    }
}