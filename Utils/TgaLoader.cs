using UnityEngine;

namespace MoreWeapons.Utils;

internal static class TgaLoader
{
    internal static Texture2D LoadFromBytes(byte[] data, string textureName = "MoreWeapons_Tga")
    {
        if (data == null || data.Length < 18)
            return null;

        var idLength = data[0];
        if (data[2] != 2)
            return null;

        var width = System.BitConverter.ToUInt16(data, 12);
        var height = System.BitConverter.ToUInt16(data, 14);
        var bpp = data[16];
        if (width <= 0 || height <= 0)
            return null;

        var bytesPerPixel = bpp / 8;
        if (bytesPerPixel != 3 && bytesPerPixel != 4)
            return null;

        var originTop = (data[17] & 0x20) != 0;
        var offset = 18 + idLength;
        var expectedSize = width * height * bytesPerPixel;
        if (data.Length < offset + expectedSize)
            return null;

        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            name = textureName
        };

        var pixels = new Color32[width * height];
        for (var y = 0; y < height; y++)
        {
            var srcY = originTop ? y : height - 1 - y;
            for (var x = 0; x < width; x++)
            {
                var srcIndex = offset + (srcY * width + x) * bytesPerPixel;
                var b = data[srcIndex];
                var g = data[srcIndex + 1];
                var r = data[srcIndex + 2];
                byte a;
                if (bytesPerPixel == 4)
                {
                    a = data[srcIndex + 3];
                }
                else
                {
                    var luminance = Mathf.Max(r, Mathf.Max(g, b));
                    a = (byte)Mathf.Clamp(luminance * 1.08f + 10f, 0f, 255f);
                }

                pixels[y * width + x] = new Color32(r, g, b, a);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }
}
