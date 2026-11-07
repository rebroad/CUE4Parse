using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse_Conversion.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CUE4Parse_Conversion.Textures.Paper2D;

internal static class TextureImageSharpAdapter
{
    public static Image<Rgba32> ToImage(this CTexture texture)
    {
        using var bitmap = texture.ToSkBitmap();
        var image = new Image<Rgba32>(bitmap.Width, bitmap.Height);
        for (var y = 0; y < bitmap.Height; y++)
        for (var x = 0; x < bitmap.Width; x++)
        {
            var pixel = bitmap.GetPixel(x, y);
            image[x, y] = new Rgba32(pixel.Red, pixel.Green, pixel.Blue, pixel.Alpha);
        }

        return image;
    }
}
