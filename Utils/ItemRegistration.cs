#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.ItemFramework;
#else
using ScheduleOne;
using ScheduleOne.ItemFramework;
#endif

using S1API.Items;
using UnityEngine;

namespace MoreWeapons.Utils
{
    internal static class ItemRegistration
    {
        public static IntegerItemDefinition RegisterRocketLauncher(Equippable equippableWrapper)
        {
            if (Registry.ItemExists(Core.RocketLauncherItemId))
                return Registry.GetItem<IntegerItemDefinition>(Core.RocketLauncherItemId);

            var template = Registry.GetItem(Core.TemplateItemId) as IntegerItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error($"Template '{Core.TemplateItemId}' was not found or is not an integer weapon.");
                return null;
            }

            var equippable = S1ApiHelpers.ResolveEquippable(equippableWrapper);
            if (equippable == null)
            {
                MelonLoader.MelonLogger.Error("Failed to resolve rocket launcher equippable prefab.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.RocketLauncherItemId;
            definition.ID = Core.RocketLauncherItemId;
            definition.Name = "Rocket Launcher";
            definition.Description = "Fires one explosive rocket at a time. Right-click to aim, left-click to fire. Press R to reload with rockets.";
            definition.StackLimit = 1;
            definition.BasePurchasePrice = Core.RocketLauncherPurchasePrice;
            definition.ResellMultiplier = 0.5f;
            definition.Equippable = equippable;
            definition.EquipMode = ScheduleOne.ItemFramework.ItemDefinition.EEquipMode.Legacy;
            definition.DefaultValue = 1;
            definition.CustomItemUI = template.CustomItemUI;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.rocket_launcher.png", "Assets/Icons/rocket_launcher.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.Baron5);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }

        public static ScheduleOne.ItemFramework.StorableItemDefinition RegisterRocketAmmo()
        {
            if (Registry.ItemExists(Core.RocketAmmoItemId))
                return Registry.GetItem<ScheduleOne.ItemFramework.StorableItemDefinition>(Core.RocketAmmoItemId);

            var template = Registry.GetItem("shotgunshell") as ScheduleOne.ItemFramework.StorableItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error("Template 'shotgunshell' was not found for rocket ammo.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.RocketAmmoItemId;
            definition.ID = Core.RocketAmmoItemId;
            definition.Name = "Rocket";
            definition.Description = "Ammunition for the rocket launcher.";
            definition.StackLimit = 10;
            definition.BasePurchasePrice = Core.RocketAmmoPurchasePrice;
            definition.ResellMultiplier = 0.4f;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.rocket.png", "Assets/Icons/rocket.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.Baron5);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }

        public static IntegerItemDefinition RegisterAk47(Equippable equippableWrapper)
        {
            if (Registry.ItemExists(Core.Ak47ItemId))
                return Registry.GetItem<IntegerItemDefinition>(Core.Ak47ItemId);

            var template = Registry.GetItem(Core.TemplateItemId) as IntegerItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error($"Template '{Core.TemplateItemId}' was not found or is not an integer weapon.");
                return null;
            }

            var equippable = S1ApiHelpers.ResolveEquippable(equippableWrapper);
            if (equippable == null)
            {
                MelonLoader.MelonLogger.Error("Failed to resolve AK-47 equippable prefab.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.Ak47ItemId;
            definition.ID = Core.Ak47ItemId;
            definition.Name = "AK-47";
            definition.Description = "Automatic rifle. Right-click to aim, hold left-click to fire. Press R to reload with AK magazines.";
            definition.StackLimit = 1;
            definition.BasePurchasePrice = Core.Ak47PurchasePrice;
            definition.ResellMultiplier = 0.5f;
            definition.Equippable = equippable;
            definition.EquipMode = ScheduleOne.ItemFramework.ItemDefinition.EEquipMode.Legacy;
            definition.DefaultValue = Core.Ak47MagazineSize;
            definition.CustomItemUI = template.CustomItemUI;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.ak47.png", "Assets/Icons/ak47.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.BlockBoss3);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }

        public static IntegerItemDefinition RegisterAk47Magazine(Equippable equippableWrapper)
        {
            if (Registry.ItemExists(Core.Ak47MagazineItemId))
                return Registry.GetItem<IntegerItemDefinition>(Core.Ak47MagazineItemId);

            var template = Registry.GetItem(Core.Ak47MagazineTemplateItemId) as IntegerItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error($"Template '{Core.Ak47MagazineTemplateItemId}' was not found for AK magazines.");
                return null;
            }

            var equippable = S1ApiHelpers.ResolveEquippable(equippableWrapper);
            if (equippable == null)
            {
                MelonLoader.MelonLogger.Error("Failed to resolve magazine equippable prefab.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.Ak47MagazineItemId;
            definition.ID = Core.Ak47MagazineItemId;
            definition.Name = "AK Magazine";
            definition.Description = "30-round magazine for the AK-47.";
            definition.StackLimit = template.StackLimit;
            definition.BasePurchasePrice = Core.Ak47MagazinePurchasePrice;
            definition.ResellMultiplier = 0.4f;
            definition.DefaultValue = Core.Ak47MagazineSize;
            definition.Equippable = equippable;
            definition.EquipMode = ScheduleOne.ItemFramework.ItemDefinition.EEquipMode.Legacy;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.ak47_mag.png", "Assets/Icons/ak47_mag.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.BlockBoss3);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }

        public static IntegerItemDefinition RegisterSniper(Equippable equippableWrapper)
        {
            if (Registry.ItemExists(Core.SniperItemId))
                return Registry.GetItem<IntegerItemDefinition>(Core.SniperItemId);

            var template = Registry.GetItem(Core.TemplateItemId) as IntegerItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error($"Template '{Core.TemplateItemId}' was not found for sniper.");
                return null;
            }

            var equippable = S1ApiHelpers.ResolveEquippable(equippableWrapper);
            if (equippable == null)
            {
                MelonLoader.MelonLogger.Error("Failed to resolve sniper equippable prefab.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.SniperItemId;
            definition.ID = Core.SniperItemId;
            definition.Name = "Sniper Rifle";
            definition.Description = "Bolt-action sniper. Scope in with right-click, cock before each shot, press R to reload magazines.";
            definition.StackLimit = 1;
            definition.BasePurchasePrice = Core.SniperPurchasePrice;
            definition.ResellMultiplier = 0.5f;
            definition.Equippable = equippable;
            definition.EquipMode = ScheduleOne.ItemFramework.ItemDefinition.EEquipMode.Legacy;
            definition.DefaultValue = Core.SniperMagazineSize;
            definition.CustomItemUI = template.CustomItemUI;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.sniper.png", "Assets/Icons/sniper.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.Underlord5);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }

        public static IntegerItemDefinition RegisterSniperMagazine(Equippable equippableWrapper)
        {
            if (Registry.ItemExists(Core.SniperMagazineItemId))
                return Registry.GetItem<IntegerItemDefinition>(Core.SniperMagazineItemId);

            var template = Registry.GetItem(Core.Ak47MagazineTemplateItemId) as IntegerItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error($"Template '{Core.Ak47MagazineTemplateItemId}' was not found for sniper magazines.");
                return null;
            }

            var equippable = S1ApiHelpers.ResolveEquippable(equippableWrapper);
            if (equippable == null)
            {
                MelonLoader.MelonLogger.Error("Failed to resolve magazine equippable prefab.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.SniperMagazineItemId;
            definition.ID = Core.SniperMagazineItemId;
            definition.Name = "Sniper Magazine";
            definition.Description = "5-round magazine for the sniper rifle.";
            definition.StackLimit = template.StackLimit;
            definition.BasePurchasePrice = Core.SniperMagazinePurchasePrice;
            definition.ResellMultiplier = 0.4f;
            definition.DefaultValue = Core.SniperMagazineSize;
            definition.Equippable = equippable;
            definition.EquipMode = ScheduleOne.ItemFramework.ItemDefinition.EEquipMode.Legacy;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.sniper_mag.png", "Assets/Icons/sniper_mag.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.Underlord5);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }

        public static ScheduleOne.ItemFramework.StorableItemDefinition RegisterGrenade(Equippable equippableWrapper)
        {
            if (Registry.ItemExists(Core.GrenadeItemId))
                return Registry.GetItem<ScheduleOne.ItemFramework.StorableItemDefinition>(Core.GrenadeItemId);

            var template = Registry.GetItem(Core.GrenadeTemplateItemId) as ScheduleOne.ItemFramework.StorableItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error($"Template '{Core.GrenadeTemplateItemId}' was not found for grenades.");
                return null;
            }

            var equippable = S1ApiHelpers.ResolveEquippable(equippableWrapper);
            if (equippable == null)
            {
                MelonLoader.MelonLogger.Error("Failed to resolve grenade equippable prefab.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.GrenadeItemId;
            definition.ID = Core.GrenadeItemId;
            definition.Name = "Grenade";
            definition.Description = "Throwable explosive. Left-click to throw.";
            definition.StackLimit = 5;
            definition.BasePurchasePrice = Core.GrenadePurchasePrice;
            definition.ResellMultiplier = 0.45f;
            definition.Equippable = equippable;
            definition.EquipMode = ScheduleOne.ItemFramework.ItemDefinition.EEquipMode.Legacy;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.grenade.png", "Assets/Icons/grenade.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.ShotCaller3);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }

        public static ScheduleOne.ItemFramework.StorableItemDefinition RegisterFlashbang(Equippable equippableWrapper)
        {
            if (Registry.ItemExists(Core.FlashbangItemId))
                return Registry.GetItem<ScheduleOne.ItemFramework.StorableItemDefinition>(Core.FlashbangItemId);

            var template = Registry.GetItem(Core.GrenadeTemplateItemId) as ScheduleOne.ItemFramework.StorableItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error($"Template '{Core.GrenadeTemplateItemId}' was not found for flashbangs.");
                return null;
            }

            var equippable = S1ApiHelpers.ResolveEquippable(equippableWrapper);
            if (equippable == null)
            {
                MelonLoader.MelonLogger.Error("Failed to resolve flashbang equippable prefab.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.FlashbangItemId;
            definition.ID = Core.FlashbangItemId;
            definition.Name = "Flashbang";
            definition.Description = "Throwable stun grenade. Blinds and deafens targets facing the burst.";
            definition.StackLimit = 5;
            definition.BasePurchasePrice = 350f;
            definition.ResellMultiplier = 0.45f;
            definition.Equippable = equippable;
            definition.EquipMode = ScheduleOne.ItemFramework.ItemDefinition.EEquipMode.Legacy;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.flashbang.png", "Assets/Icons/flashbang.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.Hustler3);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }

        public static ScheduleOne.ItemFramework.StorableItemDefinition RegisterSmokeGrenade(Equippable equippableWrapper)
        {
            if (Registry.ItemExists(Core.SmokeGrenadeItemId))
                return Registry.GetItem<ScheduleOne.ItemFramework.StorableItemDefinition>(Core.SmokeGrenadeItemId);

            var template = Registry.GetItem(Core.GrenadeTemplateItemId) as ScheduleOne.ItemFramework.StorableItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error($"Template '{Core.GrenadeTemplateItemId}' was not found for smoke grenades.");
                return null;
            }

            var equippable = S1ApiHelpers.ResolveEquippable(equippableWrapper);
            if (equippable == null)
            {
                MelonLoader.MelonLogger.Error("Failed to resolve smoke grenade equippable prefab.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.SmokeGrenadeItemId;
            definition.ID = Core.SmokeGrenadeItemId;
            definition.Name = "Smoke Grenade";
            definition.Description = "Throwable smoke screen. Left-click to throw.";
            definition.StackLimit = 5;
            definition.BasePurchasePrice = 450f;
            definition.ResellMultiplier = 0.45f;
            definition.Equippable = equippable;
            definition.EquipMode = ScheduleOne.ItemFramework.ItemDefinition.EEquipMode.Legacy;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.smoke_grenade.png", "Assets/Icons/smoke_grenade.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.Enforcer3);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }

        public static IntegerItemDefinition RegisterM4a1(Equippable equippableWrapper)
        {
            if (Registry.ItemExists(Core.M4a1ItemId))
                return Registry.GetItem<IntegerItemDefinition>(Core.M4a1ItemId);

            var template = Registry.GetItem(Core.TemplateItemId) as IntegerItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error($"Template '{Core.TemplateItemId}' was not found for M4A1.");
                return null;
            }

            var equippable = S1ApiHelpers.ResolveEquippable(equippableWrapper);
            if (equippable == null)
            {
                MelonLoader.MelonLogger.Error("Failed to resolve M4A1 equippable prefab.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.M4a1ItemId;
            definition.ID = Core.M4a1ItemId;
            definition.Name = "M4A1";
            definition.Description = "5.56 carbine. Accurate automatic rifle with mild recoil. Press R to reload STANAG magazines.";
            definition.StackLimit = 1;
            definition.BasePurchasePrice = Core.M4a1PurchasePrice;
            definition.ResellMultiplier = 0.5f;
            definition.Equippable = equippable;
            definition.EquipMode = ScheduleOne.ItemFramework.ItemDefinition.EEquipMode.Legacy;
            definition.DefaultValue = Core.M4a1MagazineSize;
            definition.CustomItemUI = template.CustomItemUI;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.m4a1.png", "Assets/Icons/m4a1.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.BlockBoss5);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }

        public static IntegerItemDefinition RegisterM4a1Magazine(Equippable equippableWrapper)
        {
            if (Registry.ItemExists(Core.M4a1MagazineItemId))
                return Registry.GetItem<IntegerItemDefinition>(Core.M4a1MagazineItemId);

            var template = Registry.GetItem(Core.Ak47MagazineTemplateItemId) as IntegerItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error($"Template '{Core.Ak47MagazineTemplateItemId}' was not found for M4 magazines.");
                return null;
            }

            var equippable = S1ApiHelpers.ResolveEquippable(equippableWrapper);
            if (equippable == null)
            {
                MelonLoader.MelonLogger.Error("Failed to resolve magazine equippable prefab.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.M4a1MagazineItemId;
            definition.ID = Core.M4a1MagazineItemId;
            definition.Name = "M4 Magazine";
            definition.Description = "30-round STANAG magazine for the M4A1.";
            definition.StackLimit = template.StackLimit;
            definition.BasePurchasePrice = Core.M4a1MagazinePurchasePrice;
            definition.ResellMultiplier = 0.4f;
            definition.DefaultValue = Core.M4a1MagazineSize;
            definition.Equippable = equippable;
            definition.EquipMode = ScheduleOne.ItemFramework.ItemDefinition.EEquipMode.Legacy;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.m4a1_mag.png", "Assets/Icons/m4a1_mag.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.BlockBoss5);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }

        public static IntegerItemDefinition RegisterDrumGun(Equippable equippableWrapper)
        {
            if (Registry.ItemExists(Core.DrumGunItemId))
                return Registry.GetItem<IntegerItemDefinition>(Core.DrumGunItemId);

            var template = Registry.GetItem(Core.TemplateItemId) as IntegerItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error($"Template '{Core.TemplateItemId}' was not found for drum gun.");
                return null;
            }

            var equippable = S1ApiHelpers.ResolveEquippable(equippableWrapper);
            if (equippable == null)
            {
                MelonLoader.MelonLogger.Error("Failed to resolve drum gun equippable prefab.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.DrumGunItemId;
            definition.ID = Core.DrumGunItemId;
            definition.Name = "Drum Gun";
            definition.Description = "High-capacity automatic. Spray and pray with drum magazines. Press R to reload.";
            definition.StackLimit = 1;
            definition.BasePurchasePrice = Core.DrumGunPurchasePrice;
            definition.ResellMultiplier = 0.5f;
            definition.Equippable = equippable;
            definition.EquipMode = ScheduleOne.ItemFramework.ItemDefinition.EEquipMode.Legacy;
            definition.DefaultValue = Core.DrumGunMagazineSize;
            definition.CustomItemUI = template.CustomItemUI;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.drumgun.png", "Assets/Icons/drumgun.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.ShotCaller3);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }

        public static IntegerItemDefinition RegisterDrumGunMagazine(Equippable equippableWrapper)
        {
            if (Registry.ItemExists(Core.DrumGunMagazineItemId))
                return Registry.GetItem<IntegerItemDefinition>(Core.DrumGunMagazineItemId);

            var template = Registry.GetItem(Core.Ak47MagazineTemplateItemId) as IntegerItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error($"Template '{Core.Ak47MagazineTemplateItemId}' was not found for drum magazines.");
                return null;
            }

            var equippable = S1ApiHelpers.ResolveEquippable(equippableWrapper);
            if (equippable == null)
            {
                MelonLoader.MelonLogger.Error("Failed to resolve magazine equippable prefab.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.DrumGunMagazineItemId;
            definition.ID = Core.DrumGunMagazineItemId;
            definition.Name = "Drum";
            definition.Description = "50-round drum for the drum gun.";
            definition.StackLimit = template.StackLimit;
            definition.BasePurchasePrice = Core.DrumGunMagazinePurchasePrice;
            definition.ResellMultiplier = 0.4f;
            definition.DefaultValue = Core.DrumGunMagazineSize;
            definition.Equippable = equippable;
            definition.EquipMode = ScheduleOne.ItemFramework.ItemDefinition.EEquipMode.Legacy;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.drum.png", "Assets/Icons/drum.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.ShotCaller3);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }

        public static IntegerItemDefinition RegisterNukeRemote(Equippable equippableWrapper)
        {
            if (Registry.ItemExists(Core.NdtItemId))
                return Registry.GetItem<IntegerItemDefinition>(Core.NdtItemId);

            var template = Registry.GetItem(Core.GrenadeEquippableTemplateItemId) as IntegerItemDefinition;
            if (template == null)
            {
                MelonLoader.MelonLogger.Error($"Template '{Core.GrenadeEquippableTemplateItemId}' was not found for N.D.T.");
                return null;
            }

            var equippable = S1ApiHelpers.ResolveEquippable(equippableWrapper);
            if (equippable == null)
            {
                MelonLoader.MelonLogger.Error("Failed to resolve N.D.T. equippable prefab.");
                return null;
            }

            var definition = UnityEngine.Object.Instantiate(template);
            definition.name = Core.NdtItemId;
            definition.ID = Core.NdtItemId;
            definition.Name = "N.D.T.";
            definition.Description = "Nuclear Delivery Terminal. Eight pre-authorized strikes — no reload. Buy a new unit when empty.";
            definition.StackLimit = 1;
            definition.BasePurchasePrice = Core.NdtPurchasePrice;
            definition.ResellMultiplier = 0.35f;
            definition.DefaultValue = Core.NdtCharges;
            definition.Equippable = equippable;
            definition.EquipMode = ScheduleOne.ItemFramework.ItemDefinition.EEquipMode.Legacy;
            definition.CustomItemUI = template.CustomItemUI;
            definition.Icon = ModAssetLoader.LoadSprite("MoreWeapons.Assets.Icons.rdu.png", "Assets/Icons/rdu.png") ?? template.Icon;
            WeaponRanks.ApplyRankGate(definition, WeaponRanks.Kingpin5);
            UnityEngine.Object.DontDestroyOnLoad(definition);
            Registry.Instance.AddToRegistry(definition);
            return definition;
        }
    }
}
