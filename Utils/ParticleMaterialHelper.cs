using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ParticleMaterialHelper
{
    internal static Material CreateSoftParticleMaterial(Color tint)
    {
        var shader = Shader.Find("Unlit/Transparent")
                     ?? Shader.Find("Sprites/Default")
                     ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended")
                     ?? Shader.Find("Particles/Standard Unlit")
                     ?? Shader.Find("Unlit/Color")
                     ?? Shader.Find("UI/Default");

        var texture = CreateSoftCircle(64, tint, new Color(tint.r, tint.g, tint.b, 0f));
        var material = new Material(shader)
        {
            mainTexture = texture,
            color = Color.white
        };
        ConfigureTransparentMaterial(material, Color.white);
        return material;
    }

    internal static Material CreateFlipbookMaterial(Texture2D texture)
    {
        var shader = Shader.Find("Unlit/Transparent")
                     ?? Shader.Find("Sprites/Default")
                     ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended")
                     ?? Shader.Find("Particles/Standard Unlit")
                     ?? Shader.Find("Unlit/Color")
                     ?? Shader.Find("UI/Default");

        var material = new Material(shader)
        {
            mainTexture = texture,
            color = Color.white
        };
        ConfigureTransparentMaterial(material, Color.white);
        return material;
    }

    internal static Material CreateProceduralSmokeParticleMaterial(Color tint, bool hotCore)
    {
        var shader = Shader.Find("Unlit/Transparent")
                     ?? Shader.Find("Sprites/Default")
                     ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended")
                     ?? Shader.Find("Particles/Standard Unlit")
                     ?? Shader.Find("Unlit/Color")
                     ?? Shader.Find("UI/Default");

        var texture = CreateProceduralSmokeAtlas(128, 4, 2, tint, hotCore);
        var material = new Material(shader)
        {
            mainTexture = texture,
            color = Color.white
        };
        ConfigureTransparentMaterial(material, Color.white);
        return material;
    }

    private static void ConfigureTransparentMaterial(Material material, Color color)
    {
        material.renderQueue = 3000;
        material.color = color;
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        if (material.HasProperty("_SrcBlend"))
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (material.HasProperty("_DstBlend"))
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (material.HasProperty("_ZWrite"))
            material.SetInt("_ZWrite", 0);

        material.DisableKeyword("FOG_LINEAR");
        material.DisableKeyword("FOG_EXP");
        material.DisableKeyword("FOG_EXP2");
    }

    private static Texture2D CreateSoftCircle(int size, Color inner, Color outer)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            name = "MoreWeapons_SoftCircle"
        };

        var center = (size - 1) * 0.5f;
        var maxRadius = center;
        var pixels = new Color[size * size];

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var dist = Mathf.Sqrt(dx * dx + dy * dy) / maxRadius;
                var alpha = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(dist));
                var color = Color.Lerp(inner, outer, Mathf.Clamp01(dist));
                color.a *= alpha;
                pixels[y * size + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static Texture2D CreateProceduralSmokeAtlas(int tileSize, int tilesX, int tilesY, Color tint, bool hotCore)
    {
        var texture = new Texture2D(tileSize * tilesX, tileSize * tilesY, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            name = hotCore ? "MoreWeapons_ProceduralHotSmokeAtlas" : "MoreWeapons_ProceduralSmokeAtlas"
        };

        var pixels = new Color[tileSize * tilesX * tileSize * tilesY];
        for (var tileY = 0; tileY < tilesY; tileY++)
        {
            for (var tileX = 0; tileX < tilesX; tileX++)
            {
                var variant = tileY * tilesX + tileX;
                WriteSmokeTile(pixels, tileSize, tilesX, tileX, tileY, variant, tint, hotCore);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static void WriteSmokeTile(
        Color[] pixels,
        int tileSize,
        int tilesX,
        int tileX,
        int tileY,
        int variant,
        Color tint,
        bool hotCore)
    {
        var atlasWidth = tileSize * tilesX;
        var center = (tileSize - 1) * 0.5f;
        var hot = new Color(1f, 0.72f, 0.18f, 1f);
        var seed = 37.1f + variant * 19.73f;

        for (var y = 0; y < tileSize; y++)
        {
            for (var x = 0; x < tileSize; x++)
            {
                var nx = (x - center) / center;
                var ny = (y - center) / center;

                var coarse = FractalNoise(x * 0.024f + seed, y * 0.024f + seed * 1.7f);
                var fine = FractalNoise(x * 0.085f + seed * 2.3f, y * 0.085f + seed * 0.43f);
                var warpX = (coarse - 0.5f) * 0.34f;
                var warpY = (fine - 0.5f) * 0.28f;
                var wx = nx + warpX + Mathf.Sin((ny + seed) * 3.7f) * 0.045f;
                var wy = ny + warpY + Mathf.Cos((nx - seed) * 4.1f) * 0.045f;

                var field = BlobContribution(wx, wy, -0.22f, 0.05f, 0.78f, 0.55f, seed * 0.02f);
                field = Mathf.Max(field, BlobContribution(wx, wy, 0.28f, -0.1f, 0.58f, 0.42f, seed * 0.031f + 0.9f));
                field = Mathf.Max(field, BlobContribution(wx, wy, -0.05f, 0.32f, 0.52f, 0.36f, seed * 0.041f + 1.7f));
                field = Mathf.Max(field, BlobContribution(wx, wy, 0.08f, -0.38f, 0.46f, 0.34f, seed * 0.053f + 2.4f));
                if ((variant & 1) == 0)
                    field = Mathf.Max(field, BlobContribution(wx, wy, -0.42f, -0.22f, 0.36f, 0.3f, seed * 0.067f + 1.2f));
                else
                    field = Mathf.Max(field, BlobContribution(wx, wy, 0.45f, 0.2f, 0.34f, 0.32f, seed * 0.071f + 2.8f));

                field += (coarse - 0.5f) * 0.16f + (fine - 0.5f) * 0.08f;
                var softEdge = Mathf.SmoothStep(-0.34f, 0.28f, field);
                var denseInterior = Mathf.SmoothStep(0.04f, 0.5f, field);
                var borderFade = 1f - Mathf.SmoothStep(0.84f, 1f, Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny)));
                var textureVariation = Mathf.Lerp(0.9f, 1.08f, fine);
                var alpha = Mathf.Clamp01((softEdge * 0.72f + denseInterior * 0.3f) * textureVariation * borderFade);

                var centerHeat = 1f - Mathf.SmoothStep(0.08f, 0.72f, Mathf.Sqrt(wx * wx + wy * wy));
                var smokeColor = Color.Lerp(tint * 0.8f, tint * 1.08f, Mathf.Clamp01(coarse * 1.05f));
                smokeColor.a = 1f;
                var color = hotCore ? Color.Lerp(smokeColor, hot, centerHeat * 0.65f) : smokeColor;
                color.a = alpha * Mathf.Lerp(0.9f, 1f, centerHeat);
                var atlasX = tileX * tileSize + x;
                var atlasY = tileY * tileSize + y;
                pixels[atlasY * atlasWidth + atlasX] = color;
            }
        }
    }

    private static float FractalNoise(float x, float y)
    {
        var amplitude = 0.55f;
        var frequency = 1f;
        var value = 0f;
        var normalizer = 0f;

        for (var octave = 0; octave < 4; octave++)
        {
            value += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
            normalizer += amplitude;
            amplitude *= 0.5f;
            frequency *= 2.05f;
        }

        return normalizer > 0f ? value / normalizer : 0f;
    }

    private static float BlobContribution(float x, float y, float centerX, float centerY, float radiusX, float radiusY, float rotation)
    {
        var sin = Mathf.Sin(rotation);
        var cos = Mathf.Cos(rotation);
        var dx = x - centerX;
        var dy = y - centerY;
        var rx = (dx * cos - dy * sin) / Mathf.Max(radiusX, 0.001f);
        var ry = (dx * sin + dy * cos) / Mathf.Max(radiusY, 0.001f);
        return 1f - Mathf.Sqrt(rx * rx + ry * ry);
    }
}
