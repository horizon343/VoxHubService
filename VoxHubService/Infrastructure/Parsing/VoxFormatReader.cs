using System.Text;
using VoxHubService.Infrastructure.Interfaces;
using VoxHubService.Infrastructure.Models.Vox;

namespace VoxHubService.Infrastructure.Parsing;

public sealed class VoxFormatReader : IVoxelFormatReader
{
    public string FormatId => "vox";

    public VoxDocument Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("Stream must be readable.", nameof(stream));

        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);

        var magic = ReadChunkId(reader);
        if (magic != "VOX ")
            throw new InvalidDataException($"Invalid .vox magic '{magic}'. Expected 'VOX '.");

        var version = reader.ReadInt32();
        if (version < 150)
            throw new InvalidDataException($"Unsupported .vox version {version}. Expected version 150 or newer.");

        var mainId = ReadChunkId(reader);
        if (mainId != "MAIN")
            throw new InvalidDataException($"Invalid root chunk '{mainId}'. Expected 'MAIN'.");

        var mainContentSize = reader.ReadInt32();
        var mainChildrenSize = reader.ReadInt32();

        if (mainContentSize != 0)
            throw new InvalidDataException("MAIN chunk content size must be 0.");

        var mainChildren = ReadExactly(reader, mainChildrenSize);

        var models = new List<VoxModel>();
        uint[]? palette = null;
        int? expectedModelCount = null;
        Size3? pendingSize = null;

        using (var childrenStream = new MemoryStream(mainChildren, writable: false))
        using (var childrenReader = new BinaryReader(childrenStream, Encoding.ASCII, leaveOpen: true))
        {
            while (childrenStream.Position < childrenStream.Length)
            {
                var chunkId = ReadChunkId(childrenReader);
                var contentSize = childrenReader.ReadInt32();
                var childChunksSize = childrenReader.ReadInt32();

                var content = ReadExactly(childrenReader, contentSize);

                if (childChunksSize > 0)
                {
                    // In standard .vox files these child bytes are usually 0 for the chunks we care about.
                    _ = ReadExactly(childrenReader, childChunksSize);
                }

                using var contentStream = new MemoryStream(content, writable: false);
                using var contentReader = new BinaryReader(contentStream, Encoding.ASCII, leaveOpen: true);

                switch (chunkId)
                {
                    case "PACK":
                    {
                        if (contentSize != 4)
                            throw new InvalidDataException("PACK chunk must contain exactly one int32.");

                        expectedModelCount = contentReader.ReadInt32();
                        if (expectedModelCount < 0)
                            throw new InvalidDataException("PACK model count cannot be negative.");
                        break;
                    }

                    case "SIZE":
                    {
                        if (contentSize != 12)
                            throw new InvalidDataException("SIZE chunk must contain exactly 3 int32 values.");

                        pendingSize = new Size3(
                            contentReader.ReadInt32(),
                            contentReader.ReadInt32(),
                            contentReader.ReadInt32()
                        );
                        break;
                    }

                    case "XYZI":
                    {
                        if (pendingSize is null)
                            throw new InvalidDataException("XYZI chunk encountered before SIZE chunk.");

                        if (contentSize < 4)
                            throw new InvalidDataException("XYZI chunk is too small.");

                        var voxelCount = contentReader.ReadInt32();
                        if (voxelCount < 0)
                            throw new InvalidDataException("Voxel count cannot be negative.");

                        var voxels = new List<VoxelCell>(voxelCount);

                        for (var i = 0; i < voxelCount; i++)
                        {
                            if (contentStream.Position + 4 > contentStream.Length)
                                throw new InvalidDataException("Unexpected end of XYZI voxel data.");

                            var x = contentReader.ReadByte();
                            var y = contentReader.ReadByte();
                            var z = contentReader.ReadByte();
                            var colorIndex = contentReader.ReadByte();

                            voxels.Add(new VoxelCell(x, y, z, colorIndex));
                        }

                        models.Add(new VoxModel(
                            pendingSize.Value.X,
                            pendingSize.Value.Y,
                            pendingSize.Value.Z,
                            voxels
                        ));

                        pendingSize = null;
                        break;
                    }

                    case "RGBA":
                    {
                        if (contentSize != 256 * 4)
                            throw new InvalidDataException("RGBA chunk must contain exactly 256 RGBA entries.");

                        var colors = new uint[256];

                        for (var i = 0; i < 256; i++)
                        {
                            var r = contentReader.ReadByte();
                            var g = contentReader.ReadByte();
                            var b = contentReader.ReadByte();
                            var a = contentReader.ReadByte();

                            colors[i] = ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | b;
                        }

                        palette = colors;
                        break;
                    }

                    default:
                        // Unknown chunk: ignore it for now.
                        break;
                }
            }
        }

        if (expectedModelCount.HasValue && expectedModelCount.Value != models.Count)
        {
            throw new InvalidDataException(
                $"PACK says {expectedModelCount.Value} models, but parsed {models.Count} SIZE/XYZI pairs."
            );
        }

        return new VoxDocument(version, models, palette);
    }

    private static string ReadChunkId(BinaryReader reader)
    {
        var bytes = ReadExactly(reader, 4);
        return Encoding.ASCII.GetString(bytes);
    }

    private static byte[] ReadExactly(BinaryReader reader, int count)
    {
        if (count < 0)
            throw new InvalidDataException("Negative byte count is invalid.");

        var buffer = reader.ReadBytes(count);
        if (buffer.Length != count)
            throw new EndOfStreamException($"Unexpected end of file while reading {count} bytes.");

        return buffer;
    }

    private readonly record struct Size3(int X, int Y, int Z);
}