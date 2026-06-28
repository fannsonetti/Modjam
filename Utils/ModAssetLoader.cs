using System.IO;
using System.Reflection;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ModAssetLoader
{
    internal static byte[] LoadBytes(string embeddedResourceName, string relativePathFromModDir)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream(embeddedResourceName))
        {
            if (stream != null)
            {
                using var memory = new MemoryStream();
                stream.CopyTo(memory);
                return memory.ToArray();
            }
        }

        var modDir = Path.GetDirectoryName(assembly.Location);
        if (string.IsNullOrEmpty(modDir))
            return null;

        var filePath = Path.Combine(modDir, relativePathFromModDir.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(filePath) ? File.ReadAllBytes(filePath) : null;
    }

    internal static Sprite LoadSprite(string embeddedResourceName, string relativePathFromModDir)
    {
        var texture = LoadTexture(embeddedResourceName, relativePathFromModDir);
        if (texture == null)
            return null;

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
    }

    internal static Texture2D LoadTexture(string embeddedResourceName, string relativePathFromModDir)
    {
        var bytes = LoadBytes(embeddedResourceName, relativePathFromModDir);
        if (bytes == null || bytes.Length == 0)
            return null;

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        return ImageConversion.LoadImage(texture, bytes) ? texture : null;
    }

    internal static Texture2D LoadTgaTexture(string embeddedResourceName, string relativePathFromModDir)
    {
        var bytes = LoadBytes(embeddedResourceName, relativePathFromModDir);
        if (bytes == null || bytes.Length == 0)
            return null;

        var name = Path.GetFileNameWithoutExtension(relativePathFromModDir);
        return TgaLoader.LoadFromBytes(bytes, $"MoreWeapons_{name}");
    }
}
