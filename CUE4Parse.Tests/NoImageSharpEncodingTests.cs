using System.Buffers.Binary;
using System.IO.Compression;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse_Conversion.Dto;
using CUE4Parse_Conversion.Options;
using CUE4Parse_Conversion.Textures;

namespace CUE4Parse.Tests;

public class NoImageSharpEncodingTests
{
    [Fact]
    public void TgaUncompressedWritesTopLeftBgraPixels()
    {
        var texture = new CTexture(2, 1, EPixelFormat.PF_R8G8B8A8,
        [255, 0, 0, 255, 0, 255, 0, 128]);

        var data = texture.Encode(new ExportOptions(textureFormat: ETextureFormat.Tga, tgaRleCompression: false), out var extension);

        Assert.Equal("tga", extension);
        Assert.Equal(2, data[2]);
        Assert.Equal(0x28, data[17]);
        Assert.Equal(new byte[] { 0, 0, 255, 255, 0, 255, 0, 128 }, data[18..]);
    }

    [Fact]
    public void TgaRleWritesRunPackets()
    {
        var texture = new CTexture(2, 1, EPixelFormat.PF_R8G8B8A8,
        [10, 20, 30, 255, 10, 20, 30, 255]);

        var data = texture.Encode(new ExportOptions(textureFormat: ETextureFormat.Tga, tgaRleCompression: true), out _);

        Assert.Equal(10, data[2]);
        Assert.Equal(new byte[] { 0x81, 30, 20, 10, 255 }, data[18..]);
    }

    [Fact]
    public void LandscapeHeightmapEncodesPng16Grayscale()
    {
        var heightmap = new LandscapeHeightmap(2, 1);
        heightmap[0, 0] = 0x1234;
        heightmap[1, 0] = 0xABCD;

        var png = heightmap.EncodePng();
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, png[..8]);
        Assert.Equal(16, png[24]);
        Assert.Equal(0, png[25]);

        var idatLength = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(33, 4));
        Assert.Equal("IDAT", System.Text.Encoding.ASCII.GetString(png, 37, 4));
        using var compressed = new MemoryStream(png, 41, idatLength);
        using var zlib = new ZLibStream(compressed, CompressionMode.Decompress);
        using var pixels = new MemoryStream();
        zlib.CopyTo(pixels);

        Assert.Equal(new byte[] { 0, 0x12, 0x34, 0xAB, 0xCD }, pixels.ToArray());
    }
}
