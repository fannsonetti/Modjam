namespace MoreWeapons.Utils;

internal static class MagazineGlbCatalog
{
    internal readonly struct GlbAsset
    {
        internal GlbAsset(string embeddedResourceName, string relativePathFromModDir)
        {
            EmbeddedResourceName = embeddedResourceName;
            RelativePathFromModDir = relativePathFromModDir;
        }

        internal string EmbeddedResourceName { get; }
        internal string RelativePathFromModDir { get; }
    }

    internal static bool TryGet(string itemId, out GlbAsset asset)
    {
        asset = default;
        if (string.IsNullOrEmpty(itemId))
            return false;

        switch (itemId)
        {
            case Core.Ak47MagazineItemId:
                asset = new GlbAsset(
                    "MoreWeapons.Assets.Models.ak47_mag.glb",
                    "Assets/Models/ak47_mag.glb");
                return true;
            case Core.DrumGunMagazineItemId:
                asset = new GlbAsset(
                    "MoreWeapons.Assets.Models.drum.glb",
                    "Assets/Models/drum.glb");
                return true;
            case Core.M4a1MagazineItemId:
                asset = new GlbAsset(
                    "MoreWeapons.Assets.Models.m4a1_mag.glb",
                    "Assets/Models/m4a1_mag.glb");
                return true;
            case Core.SniperMagazineItemId:
                asset = new GlbAsset(
                    "MoreWeapons.Assets.Models.sniper_mag.glb",
                    "Assets/Models/sniper_mag.glb");
                return true;
            default:
                return false;
        }
    }
}
