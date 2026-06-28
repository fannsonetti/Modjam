using System.Collections.Generic;
using UnityEngine;

namespace MoreWeapons.Utils;

internal enum VfxFlipbookId
{
    None = 0,
    WispySmoke01,
    WispySmoke03,
    FireBall02,
    Explosion00,
    Explosion01,
    Explosion01Light,
    Explosion01NoFire,
    Explosion01LightNoFire,
    Explosion02
}

internal static class VfxFlipbookCatalog
{
    private sealed class Definition
    {
        internal string EmbeddedResourceName;
        internal string RelativePath;
        internal int TilesX;
        internal int TilesY;
    }

    private static readonly Dictionary<VfxFlipbookId, Definition> Definitions = new()
    {
        [VfxFlipbookId.WispySmoke01] = Def("WispySmoke01_8x8.tga", 8, 8),
        [VfxFlipbookId.WispySmoke03] = Def("WispySmoke03_8x8.tga", 8, 8),
        [VfxFlipbookId.FireBall02] = Def("FireBall02_8x8.tga", 8, 8),
        [VfxFlipbookId.Explosion00] = Def("Explosion00_5x5.tga", 5, 5),
        [VfxFlipbookId.Explosion01] = Def("Explosion01_5x5.tga", 5, 5),
        [VfxFlipbookId.Explosion01Light] = Def("Explosion01-light_5x5.tga", 5, 5),
        [VfxFlipbookId.Explosion01NoFire] = Def("Explosion01-nofire_5x5.tga", 5, 5),
        [VfxFlipbookId.Explosion01LightNoFire] = Def("Explosion01-light-nofire_5x5.tga", 5, 5),
        [VfxFlipbookId.Explosion02] = Def("Explosion02_5x5.tga", 5, 5)
    };

    private static readonly Dictionary<VfxFlipbookId, Texture2D> TextureCache = new();
    private static readonly Dictionary<VfxFlipbookId, Material> MaterialCache = new();

    internal static bool TryGetTiles(VfxFlipbookId id, out int tilesX, out int tilesY)
    {
        tilesX = 0;
        tilesY = 0;
        if (!Definitions.TryGetValue(id, out var definition))
            return false;

        tilesX = definition.TilesX;
        tilesY = definition.TilesY;
        return true;
    }

    internal static Material GetMaterial(VfxFlipbookId id)
    {
        if (id == VfxFlipbookId.None)
            return null;

        if (MaterialCache.TryGetValue(id, out var cached) && cached != null)
            return cached;

        var texture = GetTexture(id);
        if (texture == null)
            return null;

        var material = ParticleMaterialHelper.CreateFlipbookMaterial(texture);
        MaterialCache[id] = material;
        return material;
    }

    internal static void ApplyTextureSheetAnimation(ParticleSystem ps, VfxFlipbookId id, float frameOverLifetime = 1f)
    {
        if (ps == null || !TryGetTiles(id, out var tilesX, out var tilesY))
            return;

        var sheet = ps.textureSheetAnimation;
        sheet.enabled = true;
        sheet.mode = ParticleSystemAnimationMode.Grid;
        sheet.numTilesX = tilesX;
        sheet.numTilesY = tilesY;
        sheet.animation = ParticleSystemAnimationType.WholeSheet;
        sheet.frameOverTime = new ParticleSystem.MinMaxCurve(frameOverLifetime);
        sheet.startFrame = new ParticleSystem.MinMaxCurve(0f, 1f);
        sheet.cycleCount = 1;
    }

    private static Texture2D GetTexture(VfxFlipbookId id)
    {
        if (TextureCache.TryGetValue(id, out var cached) && cached != null)
            return cached;

        if (!Definitions.TryGetValue(id, out var definition))
            return null;

        var texture = ModAssetLoader.LoadTgaTexture(definition.EmbeddedResourceName, definition.RelativePath);
        if (texture != null)
            TextureCache[id] = texture;

        return texture;
    }

    private static Definition Def(string fileName, int tilesX, int tilesY)
    {
        return new Definition
        {
            EmbeddedResourceName = $"MoreWeapons.Assets.VFX.{fileName}",
            RelativePath = $"Assets/VFX/{fileName}",
            TilesX = tilesX,
            TilesY = tilesY
        };
    }
}
