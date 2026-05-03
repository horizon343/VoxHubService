using VoxHubService.Domain.Canonical;
using VoxHubService.Domain.Chunking;

namespace VoxHubService.Domain.Serialization;

public static class ChunkBlobCodec
{
    // Blob хранит только локальные координаты внутри чанка.
    public static byte[] Serialize(ChunkSlice chunk)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write((ushort)chunk.Voxels.Count);

        var origin = chunk.Bounds.Min;

        foreach (var v in chunk.Voxels)
        {
            var lx = v.Position.X - origin.X;
            var ly = v.Position.Y - origin.Y;
            var lz = v.Position.Z - origin.Z;

            if ((uint)lx > byte.MaxValue ||
                (uint)ly > byte.MaxValue ||
                (uint)lz > byte.MaxValue)
            {
                throw new InvalidDataException("Voxel position is outside chunk bounds.");
            }

            writer.Write((byte)lx);
            writer.Write((byte)ly);
            writer.Write((byte)lz);
            writer.Write(v.PaletteIndex);
        }

        writer.Flush();
        return stream.ToArray();
    }

    // Возвращает уже абсолютные координаты.
    public static IReadOnlyList<Voxel> Deserialize(Stream stream, Int3 origin)
    {
        using var reader = new BinaryReader(stream);

        var count = reader.ReadUInt16();
        var voxels = new Voxel[count];

        for (var i = 0; i < count; i++)
        {
            voxels[i] = new Voxel(
                new Int3(
                    origin.X + reader.ReadByte(),
                    origin.Y + reader.ReadByte(),
                    origin.Z + reader.ReadByte()),
                reader.ReadByte());
        }

        return voxels;
    }
}