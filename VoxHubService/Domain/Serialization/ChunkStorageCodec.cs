using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VoxHubService.Domain.Canonical;
using VoxHubService.Domain.Chunking;

namespace VoxHubService.Domain.Serialization;

public static class ChunkStorageCodec
{
    // В файле это будет выглядеть как bytes: V H C 2
    private const int Magic = 0x32434856;
    private const byte FormatVersion = 1;

    private const byte CoordByte = 1;
    private const byte CoordUShort = 2;
    private const byte CoordInt32 = 4;

    public static byte[] Serialize(ChunkSlice chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        var coordWidth = PickCoordinateWidth(chunk);

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write(Magic);
        writer.Write(FormatVersion);
        writer.Write(coordWidth);
        writer.Write(chunk.Voxels.Count);

        var origin = chunk.Bounds.Min;

        foreach (var voxel in chunk.Voxels)
        {
            if (!chunk.Bounds.Contains(voxel.Position))
                throw new InvalidDataException("Voxel is outside of its chunk bounds.");

            var x = voxel.Position.X - origin.X;
            var y = voxel.Position.Y - origin.Y;
            var z = voxel.Position.Z - origin.Z;

            WriteCoordinate(writer, coordWidth, x);
            WriteCoordinate(writer, coordWidth, y);
            WriteCoordinate(writer, coordWidth, z);

            writer.Write(voxel.PaletteIndex);
        }

        writer.Flush();
        return stream.ToArray();
    }

    public static IReadOnlyList<Voxel> Deserialize(Stream stream, ChunkKey key, int chunkSize)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be greater than zero.");

        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        var first = reader.ReadInt32();

        // Backward compatibility:
        // старые chunks начинались сразу с count, без magic header.
        if (first != Magic)
            return DeserializeLegacy(reader, first);

        var version = reader.ReadByte();
        if (version != FormatVersion)
            throw new InvalidDataException($"Unsupported chunk storage format version: {version}.");

        var coordWidth = reader.ReadByte();
        if (coordWidth is not CoordByte and not CoordUShort and not CoordInt32)
            throw new InvalidDataException($"Unsupported coordinate width: {coordWidth}.");

        var count = reader.ReadInt32();
        ValidateCount(count);

        var origin = key.ToOrigin(chunkSize);
        var voxels = new Voxel[count];

        for (var i = 0; i < count; i++)
        {
            var x = ReadCoordinate(reader, coordWidth);
            var y = ReadCoordinate(reader, coordWidth);
            var z = ReadCoordinate(reader, coordWidth);
            var paletteIndex = reader.ReadByte();

            voxels[i] = new Voxel(
                new Int3(origin.X + x, origin.Y + y, origin.Z + z),
                paletteIndex);
        }

        return voxels;
    }

    private static IReadOnlyList<Voxel> DeserializeLegacy(BinaryReader reader, int count)
    {
        ValidateCount(count);

        var voxels = new Voxel[count];

        for (var i = 0; i < count; i++)
        {
            voxels[i] = new Voxel(
                new Int3(
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadInt32()),
                reader.ReadByte());
        }

        return voxels;
    }

    private static byte PickCoordinateWidth(ChunkSlice chunk)
    {
        var origin = chunk.Bounds.Min;
        var maxLocal = 0;

        foreach (var voxel in chunk.Voxels)
        {
            if (!chunk.Bounds.Contains(voxel.Position))
                throw new InvalidDataException("Voxel is outside of its chunk bounds.");

            var localMax = Math.Max(
                voxel.Position.X - origin.X,
                Math.Max(
                    voxel.Position.Y - origin.Y,
                    voxel.Position.Z - origin.Z));

            if (localMax > maxLocal)
                maxLocal = localMax;
        }

        if (maxLocal <= byte.MaxValue)
            return CoordByte;

        if (maxLocal <= ushort.MaxValue)
            return CoordUShort;

        return CoordInt32;
    }

    private static void WriteCoordinate(BinaryWriter writer, byte width, int value)
    {
        if (value < 0)
            throw new InvalidDataException("Local voxel coordinate cannot be negative.");

        switch (width)
        {
            case CoordByte:
                writer.Write((byte)value);
                break;

            case CoordUShort:
                writer.Write((ushort)value);
                break;

            case CoordInt32:
                writer.Write(value);
                break;

            default:
                throw new InvalidDataException($"Unsupported coordinate width: {width}.");
        }
    }

    private static int ReadCoordinate(BinaryReader reader, byte width)
    {
        return width switch
        {
            CoordByte => reader.ReadByte(),
            CoordUShort => reader.ReadUInt16(),
            CoordInt32 => reader.ReadInt32(),
            _ => throw new InvalidDataException($"Unsupported coordinate width: {width}.")
        };
    }

    private static void ValidateCount(int count)
    {
        if (count < 0)
            throw new InvalidDataException("Voxel count cannot be negative.");
    }
}