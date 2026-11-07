using System.Buffers.Binary;
using System.IO.Compression;

namespace CUE4Parse_Conversion.Dto;

/// <summary>A 16-bit grayscale landscape heightmap.</summary>
public sealed class LandscapeHeightmap(int width, int height)
{
    private readonly ushort[] _samples = new ushort[checked(width * height)];

    public int Width { get; } = width;
    public int Height { get; } = height;

    public ushort this[int x, int y]
    {
        get => _samples[checked(y * Width + x)];
        set => _samples[checked(y * Width + x)] = value;
    }

    public byte[] EncodePng()
    {
        using var output = new MemoryStream();
        output.Write([137, 80, 78, 71, 13, 10, 26, 10]);

        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, Width);
        BinaryPrimitives.WriteInt32BigEndian(header[4..], Height);
        header[8] = 16; // bit depth
        header[9] = 0;  // grayscale
        WriteChunk(output, "IHDR"u8, header);

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            var row = new byte[checked(1 + Width * 2)]; // filter byte plus big-endian samples
            for (var y = 0; y < Height; y++)
            {
                row[0] = 0;
                for (var x = 0; x < Width; x++)
                    BinaryPrimitives.WriteUInt16BigEndian(row.AsSpan(1 + x * 2), this[x, y]);
                zlib.Write(row);
            }
        }

        WriteChunk(output, "IDAT"u8, compressed.GetBuffer().AsSpan(0, checked((int)compressed.Length)));
        WriteChunk(output, "IEND"u8, []);
        return output.ToArray();
    }

    private static void WriteChunk(Stream output, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        Span<byte> word = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(word, data.Length);
        output.Write(word);
        output.Write(type);
        output.Write(data);

        var crc = 0xFFFFFFFFu;
        foreach (var value in type)
            crc = UpdateCrc(crc, value);
        foreach (var value in data)
            crc = UpdateCrc(crc, value);
        BinaryPrimitives.WriteUInt32BigEndian(word, ~crc);
        output.Write(word);
    }

    private static uint UpdateCrc(uint crc, byte value)
    {
        crc ^= value;
        for (var bit = 0; bit < 8; bit++)
            crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
        return crc;
    }
}
