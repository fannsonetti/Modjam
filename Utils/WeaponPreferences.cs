using MelonLoader;

namespace MoreWeapons.Utils;

internal static class WeaponPreferences
{
    private const string Prefix = "MoreWeapons";

    internal static void Initialize()
    {
        BindAk47();
        BindM4a1();
        BindDrumGun();
        BindSniper();
        BindRocketLauncher();
        BindGrenade();
        BindNuke();
    }

    private static void BindAk47()
    {
        var cat = MelonPreferences.CreateCategory($"{Prefix}.AK47", "MoreWeapons — AK-47");
        Core.Ak47Damage = Bind(cat, "Damage", Core.Ak47Damage, "Bullet damage");
        Core.Ak47MinSpread = Bind(cat, "MinSpread", Core.Ak47MinSpread, "Minimum spread (degrees)");
        Core.Ak47MaxSpread = Bind(cat, "MaxSpread", Core.Ak47MaxSpread, "Maximum spread (degrees)");
        Core.Ak47MagazineSize = BindInt(cat, "MagazineSize", Core.Ak47MagazineSize, "Rounds per magazine");
        Core.Ak47MagazinePurchasePrice = Bind(cat, "MagazinePrice", Core.Ak47MagazinePurchasePrice, "Magazine shop price");
        Core.Ak47PurchasePrice = Bind(cat, "PurchasePrice", Core.Ak47PurchasePrice, "Shop price");
        Core.Ak47FireCooldown = Bind(cat, "FireCooldown", Core.Ak47FireCooldown, "Seconds between shots");
    }

    private static void BindM4a1()
    {
        var cat = MelonPreferences.CreateCategory($"{Prefix}.M4A1", "MoreWeapons — M4A1");
        Core.M4a1Damage = Bind(cat, "Damage", Core.M4a1Damage, "Bullet damage");
        Core.M4a1MinSpread = Bind(cat, "MinSpread", Core.M4a1MinSpread, "Minimum spread (degrees)");
        Core.M4a1MaxSpread = Bind(cat, "MaxSpread", Core.M4a1MaxSpread, "Maximum spread (degrees)");
        Core.M4a1MagazineSize = BindInt(cat, "MagazineSize", Core.M4a1MagazineSize, "Rounds per magazine");
        Core.M4a1MagazinePurchasePrice = Bind(cat, "MagazinePrice", Core.M4a1MagazinePurchasePrice, "Magazine shop price");
        Core.M4a1PurchasePrice = Bind(cat, "PurchasePrice", Core.M4a1PurchasePrice, "Shop price");
        Core.M4a1FireCooldown = Bind(cat, "FireCooldown", Core.M4a1FireCooldown, "Seconds between shots");
    }

    private static void BindDrumGun()
    {
        var cat = MelonPreferences.CreateCategory($"{Prefix}.DrumGun", "MoreWeapons — Drum Gun");
        Core.DrumGunDamage = Bind(cat, "Damage", Core.DrumGunDamage, "Bullet damage");
        Core.DrumGunMinSpread = Bind(cat, "MinSpread", Core.DrumGunMinSpread, "Minimum spread (degrees)");
        Core.DrumGunMaxSpread = Bind(cat, "MaxSpread", Core.DrumGunMaxSpread, "Maximum spread (degrees)");
        Core.DrumGunMagazineSize = BindInt(cat, "MagazineSize", Core.DrumGunMagazineSize, "Rounds per magazine");
        Core.DrumGunMagazinePurchasePrice = Bind(cat, "MagazinePrice", Core.DrumGunMagazinePurchasePrice, "Magazine shop price");
        Core.DrumGunPurchasePrice = Bind(cat, "PurchasePrice", Core.DrumGunPurchasePrice, "Shop price");
        Core.DrumGunFireCooldown = Bind(cat, "FireCooldown", Core.DrumGunFireCooldown, "Seconds between shots");
    }

    private static void BindSniper()
    {
        var cat = MelonPreferences.CreateCategory($"{Prefix}.Sniper", "MoreWeapons — Sniper");
        Core.SniperDamage = Bind(cat, "Damage", Core.SniperDamage, "Bullet damage");
        Core.SniperMinSpread = Bind(cat, "MinSpread", Core.SniperMinSpread, "Minimum spread when scoped (degrees)");
        Core.SniperMaxSpread = Bind(cat, "MaxSpread", Core.SniperMaxSpread, "Maximum spread when moving (degrees)");
        Core.SniperMagazineSize = BindInt(cat, "MagazineSize", Core.SniperMagazineSize, "Rounds per magazine");
        Core.SniperMagazinePurchasePrice = Bind(cat, "MagazinePrice", Core.SniperMagazinePurchasePrice, "Magazine shop price");
        Core.SniperPurchasePrice = Bind(cat, "PurchasePrice", Core.SniperPurchasePrice, "Shop price");
        Core.SniperFireCooldown = Bind(cat, "FireCooldown", Core.SniperFireCooldown, "Seconds between shots");
    }

    private static void BindRocketLauncher()
    {
        var cat = MelonPreferences.CreateCategory($"{Prefix}.RocketLauncher", "MoreWeapons — Rocket Launcher");
        Core.RocketMaxDamage = Bind(cat, "Damage", Core.RocketMaxDamage, "Explosion damage");
        Core.RocketMinReticleSpread = Bind(cat, "MinSpread", Core.RocketMinReticleSpread, "Reticle spread when aimed (degrees)");
        Core.RocketMaxReticleSpread = Bind(cat, "MaxSpread", Core.RocketMaxReticleSpread, "Reticle spread when hip-firing (degrees)");
        Core.RocketLauncherPurchasePrice = Bind(cat, "PurchasePrice", Core.RocketLauncherPurchasePrice, "Launcher shop price");
        Core.RocketAmmoPurchasePrice = Bind(cat, "AmmoPrice", Core.RocketAmmoPurchasePrice, "Rocket ammo shop price");
        Core.RocketFireCooldown = Bind(cat, "FireCooldown", Core.RocketFireCooldown, "Seconds between shots");
    }

    private static void BindGrenade()
    {
        var cat = MelonPreferences.CreateCategory($"{Prefix}.Grenade", "MoreWeapons — Grenade");
        Core.GrenadeMaxDamage = Bind(cat, "Damage", Core.GrenadeMaxDamage, "Explosion damage");
        Core.GrenadePurchasePrice = Bind(cat, "PurchasePrice", Core.GrenadePurchasePrice, "Shop price");
        Core.GrenadeBlastRadius = Bind(cat, "BlastRadius", Core.GrenadeBlastRadius, "Explosion radius (meters)");
    }

    private static void BindNuke()
    {
        var cat = MelonPreferences.CreateCategory($"{Prefix}.Nuke", "MoreWeapons — Nuke Strike");
        Core.NukeMaxDamage = Bind(cat, "Damage", Core.NukeMaxDamage, "Explosion damage");
        Core.NukeDealsDamage = BindBool(cat, "DealsDamage", Core.NukeDealsDamage, "Whether strikes deal damage");
        Core.NdtCharges = BindInt(cat, "Charges", Core.NdtCharges, "Strike charges per N.D.T.");
        Core.NukeStrikeCost = Bind(cat, "StrikeCost", Core.NukeStrikeCost, "Credit card cost per strike");
        Core.NdtPurchasePrice = Bind(cat, "PurchasePrice", Core.NdtPurchasePrice, "N.D.T. shop price");
    }

    private static int BindInt(MelonPreferences_Category category, string name, int defaultValue, string description)
    {
        return category.CreateEntry(name, defaultValue, name, description).Value;
    }

    private static float Bind(MelonPreferences_Category category, string name, float defaultValue, string description)
    {
        return category.CreateEntry(name, defaultValue, name, description).Value;
    }

    private static bool BindBool(MelonPreferences_Category category, string name, bool defaultValue, string description)
    {
        return category.CreateEntry(name, defaultValue, name, description).Value;
    }
}
