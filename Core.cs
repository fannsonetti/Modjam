#if IL2CPP

using Il2CppInterop.Runtime.Injection;

using Il2CppScheduleOne;

using Il2CppScheduleOne.DevUtilities;

using Il2CppScheduleOne.Persistence;

#else

using ScheduleOne;

using ScheduleOne.DevUtilities;

using ScheduleOne.Persistence;

#endif



using HarmonyLib;

using MelonLoader;

using S1API.Items;

using MoreWeapons.Patches;

using S1API.Shops;

using MoreWeapons.Utils;

using MoreWeapons.Weapons;

using GameIntegerItemDefinition = ScheduleOne.ItemFramework.IntegerItemDefinition;

using GameStorableItemDefinition = ScheduleOne.ItemFramework.StorableItemDefinition;



[assembly: MelonInfo(typeof(MoreWeapons.Core), Constants.MOD_NAME, Constants.MOD_VERSION, Constants.MOD_AUTHOR)]

[assembly: MelonGame(Constants.Game.GAME_STUDIO, Constants.Game.GAME_NAME)]



namespace MoreWeapons

{

    public sealed class Core : MelonMod

    {

        public const string RocketLauncherItemId = "rocketlauncher";

        public const string TemplateItemId = "pumpshotgun";

        public const string RocketAmmoItemId = "rocket";



        public const string Ak47ItemId = "ak47";

        public const string Ak47MagazineItemId = "ak47mag";

        public const string Ak47ReloadTemplateItemId = "m1911";

        public const string Ak47MagazineTemplateItemId = "m1911mag";

        public static int Ak47MagazineSize { get; internal set; } = 30;
        public static float Ak47MagazinePurchasePrice { get; internal set; } = 250f;



        public const string SniperItemId = "sniper";

        public const string SniperMagazineItemId = "snipermag";

        public static int SniperMagazineSize { get; internal set; } = 5;
        public static float SniperMagazinePurchasePrice { get; internal set; } = 400f;

        public const string GrenadeItemId = "grenade";
        public const string GrenadeTemplateItemId = "cuke";
        public const string GrenadeEquippableTemplateItemId = "m1911";

        public const string FlashbangItemId = "flashbang";
        public const string SmokeGrenadeItemId = "smokegrenade";

        public const string M4a1ItemId = "m4a1";
        public const string M4a1MagazineItemId = "m4a1mag";
        public static int M4a1MagazineSize { get; internal set; } = 30;
        public static float M4a1MagazinePurchasePrice { get; internal set; } = 280f;

        public const string DrumGunItemId = "drumgun";
        public const string DrumGunMagazineItemId = "drumgunmag";
        public static int DrumGunMagazineSize { get; internal set; } = 50;
        public static float DrumGunMagazinePurchasePrice { get; internal set; } = 450f;

        public const string NdtItemId = "NDT";

        public static float RocketFireCooldown { get; internal set; } = 1.35f;
        public static float RocketLauncherPurchasePrice { get; internal set; } = 50000f;
        public static float RocketAmmoPurchasePrice { get; internal set; } = 900f;

        public static float RocketInitialSpeed { get; } = 8f;
        public static float RocketMotorDelay { get; } = 1f;
        public static float RocketSpeed { get; } = 32f;
        public static float RocketAcceleration { get; } = 16f;

        public static float RocketLifetime { get; } = 8f;

        public static float RocketBlastRadius { get; } = 10f;

        public static float RocketMaxDamage { get; internal set; } = 350f;

        public static float RocketMaxPushForce { get; } = 1200f;

        public static float RocketMinReticleSpread { get; internal set; } = 1f;

        public static float RocketMaxReticleSpread { get; internal set; } = 6f;



        public static float Ak47FireCooldown { get; internal set; } = 0.095f;
        public static float Ak47PurchasePrice { get; internal set; } = 12500f;

        public static float Ak47MinSpread { get; internal set; } = 7f;

        public static float Ak47MaxSpread { get; internal set; } = 12f;

        public static float Ak47AccuracyDropPerShot { get; } = 0.42f;

        public static float Ak47Damage { get; internal set; } = 70f;

        public static float Ak47Range { get; } = 70f;

        public static float Ak47ImpactForce { get; } = 220f;



        public static float SniperFireCooldown { get; internal set; } = 0.85f;
        public static float SniperPurchasePrice { get; internal set; } = 20000f;

        public static float SniperMinSpread { get; internal set; } = 0f;

        public static float SniperMaxSpread { get; internal set; } = 2f;

        public static float SniperDamage { get; internal set; } = 150f;

        public static float SniperRange { get; } = 180f;

        public static float SniperImpactForce { get; } = 420f;

        public static float SniperHeadshotMultiplier { get; } = 2.5f;

        public static float SniperAimFovReduction { get; } = 32f;

        public static float SniperScopedFov { get; } = 11f;

        public static float SniperScopedMouseSensitivity { get; } = 0.35f;

        public static float GrenadeFuseDuration { get; } = 2.75f;

        public static float GrenadeBlastRadius { get; internal set; } = 5.5f;
        public static float GrenadePurchasePrice { get; internal set; } = 650f;

        public static float GrenadeMaxDamage { get; internal set; } = 160f;

        public static float GrenadeMaxPushForce { get; } = 700f;

        public static float FlashbangFuseDuration { get; } = 1.4f;

        public static float SmokeGrenadeFuseDuration { get; } = 2.2f;
        public static float SmokeCloudDuration { get; } = 22f;
        public static float SmokeCloudRadius { get; } = 2f;

        public static float M4a1FireCooldown { get; internal set; } = 0.08f;
        public static float M4a1PurchasePrice { get; internal set; } = 15000f;
        public static float M4a1MinSpread { get; internal set; } = 5f;
        public static float M4a1MaxSpread { get; internal set; } = 10f;
        public static float M4a1AccuracyDropPerShot { get; } = 0.22f;
        public static float M4a1Damage { get; internal set; } = 50f;
        public static float M4a1Range { get; } = 72f;
        public static float M4a1ImpactForce { get; } = 195f;

        public static float DrumGunFireCooldown { get; internal set; } = 0.1f;
        public static float DrumGunPurchasePrice { get; internal set; } = 5000f;
        public static float DrumGunMinSpread { get; internal set; } = 4f;
        public static float DrumGunMaxSpread { get; internal set; } = 11f;
        public static float DrumGunAccuracyDropPerShot { get; } = 0.58f;
        public static float DrumGunDamage { get; internal set; } = 35f;
        public static float DrumGunRange { get; } = 42f;
        public static float DrumGunImpactForce { get; } = 145f;

        public static float NukeBlastRadius { get; } = 90f;
        public static float NukeBlastVisualRadius { get; } = 110f;
        public static float NukeMaxDamage { get; internal set; } = 2500f;
        public static float NukeMaxPushForce { get; } = 12000f;
        public static float NukeCloudDuration { get; } = 90f;
        public static float NukeCloudRadius { get; } = 185f;
        public static float NukeVisualScale { get; } = 1.3f;
        public static bool NukeDealsDamage { get; internal set; } = true;
        public static float NukePlaneAltitude { get; } = 105f;
        public static float NukePlaneSpeed { get; } = 32f;
        public static float NukePlaneApproachDistance { get; } = 520f;
        public static float NukePlaneDropTolerance { get; } = 8f;
        public static float NukePlaneExitDistance { get; } = 300f;
        public static float NukeProjectileGravity { get; } = 18f;
        public static float NukeProjectileHorizontalDrag { get; } = 0.42f;
        public static float NukeStrikeCost { get; internal set; } = 50000f;
        public static int NdtCharges { get; internal set; } = 8;
        public static float NdtPurchasePrice { get; internal set; } = 50000f;
        public static float NukeStrikeDelay { get; } = 45f;



#if IL2CPP
        public static Il2CppScheduleOne.Combat.ExplosionData RocketExplosion =>
            new(RocketBlastRadius, RocketMaxDamage, RocketMaxPushForce, false);

        public static Il2CppScheduleOne.Combat.ExplosionData GrenadeExplosion =>
            new(GrenadeBlastRadius, GrenadeMaxDamage, GrenadeMaxPushForce, false);

        public static Il2CppScheduleOne.Combat.ExplosionData NukeExplosion =>
            new(NukeBlastRadius, NukeMaxDamage, NukeMaxPushForce, false);
#else
        public static ScheduleOne.Combat.ExplosionData RocketExplosion =>
            new(RocketBlastRadius, RocketMaxDamage, RocketMaxPushForce, false);

        public static ScheduleOne.Combat.ExplosionData GrenadeExplosion =>
            new(GrenadeBlastRadius, GrenadeMaxDamage, GrenadeMaxPushForce, false);

        public static ScheduleOne.Combat.ExplosionData NukeExplosion =>
            new(NukeBlastRadius, NukeMaxDamage, NukeMaxPushForce, false);
#endif



        private bool _itemsRegistered;

        private bool _loadHooked;

        private static GameIntegerItemDefinition _rocketLauncherDef;

        private static GameStorableItemDefinition _rocketAmmoDef;

        private static GameIntegerItemDefinition _ak47Def;

        private static GameIntegerItemDefinition _ak47MagDef;

        private static GameIntegerItemDefinition _sniperDef;

        private static GameIntegerItemDefinition _sniperMagDef;
        private static GameStorableItemDefinition _grenadeDef;
        private static GameStorableItemDefinition _flashbangDef;
        private static GameStorableItemDefinition _smokeGrenadeDef;
        private static GameIntegerItemDefinition _m4a1Def;
        private static GameIntegerItemDefinition _m4a1MagDef;
        private static GameIntegerItemDefinition _drumGunDef;
        private static GameIntegerItemDefinition _drumGunMagDef;
        private static GameIntegerItemDefinition _nukeRemoteDef;



        public override void OnInitializeMelon()

        {
            HarmonyInstance.PatchAll(typeof(AvatarAnimationCullingGuardPatch).Assembly);

#if IL2CPP

            ClassInjector.RegisterTypeInIl2Cpp<RocketLauncherEquippable>();

            ClassInjector.RegisterTypeInIl2Cpp<Ak47Equippable>();

            ClassInjector.RegisterTypeInIl2Cpp<SniperEquippable>();

            ClassInjector.RegisterTypeInIl2Cpp<CukeViewmodelEquippable>();
            ClassInjector.RegisterTypeInIl2Cpp<CukeThrowableEquippable>();
            ClassInjector.RegisterTypeInIl2Cpp<GrenadeEquippable>();

            ClassInjector.RegisterTypeInIl2Cpp<FlashbangEquippable>();

            ClassInjector.RegisterTypeInIl2Cpp<SmokeGrenadeEquippable>();

            ClassInjector.RegisterTypeInIl2Cpp<M4a1Equippable>();

            ClassInjector.RegisterTypeInIl2Cpp<DrumGunEquippable>();

            ClassInjector.RegisterTypeInIl2Cpp<MagazineEquippable>();

            ClassInjector.RegisterTypeInIl2Cpp<NukeRemoteEquippable>();
            ClassInjector.RegisterTypeInIl2Cpp<NukeBomberPlane>();
            ClassInjector.RegisterTypeInIl2Cpp<NukeProjectile>();
            ClassInjector.RegisterTypeInIl2Cpp<MushroomCloudEffect>();

            ClassInjector.RegisterTypeInIl2Cpp<RocketProjectile>();

            ClassInjector.RegisterTypeInIl2Cpp<GrenadeProjectile>();

            ClassInjector.RegisterTypeInIl2Cpp<FlashbangProjectile>();

            ClassInjector.RegisterTypeInIl2Cpp<SmokeGrenadeProjectile>();

            ClassInjector.RegisterTypeInIl2Cpp<SmokeCloudEffect>();

            ClassInjector.RegisterTypeInIl2Cpp<RocketTrailEffect>();

            ClassInjector.RegisterTypeInIl2Cpp<ViewmodelEditorHost>();

#endif

            WeaponPreferences.Initialize();

            ViewmodelEditor.Initialize();

            NukeMapOverlay.Initialize();

            LoggerInstance.Msg($"{Constants.MOD_NAME} initialized.");

        }



        public override void OnSceneWasInitialized(int buildIndex, string sceneName)

        {

            if (sceneName == "Main" && !_itemsRegistered)

            {

                _itemsRegistered = true;

                RegisterItems();

            }

            if (sceneName == "Main")
                ViewmodelEditorHost.Ensure();

            if (sceneName == "Main" && !_loadHooked)

            {

#if IL2CPP

                var loadManager = Singleton<LoadManager>.Instance;

#else

                var loadManager = LoadManager.Instance;

#endif

                if (loadManager != null)

                {

#if IL2CPP

                    loadManager.onLoadComplete.AddListener((UnityEngine.Events.UnityAction)OnGameLoaded);

#else

                    loadManager.onLoadComplete.AddListener(OnGameLoaded);

#endif

                    _loadHooked = true;

                }

            }

        }



        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)

        {

            if (sceneName == "Main")

                _loadHooked = false;

        }



        private static void RegisterItems()

        {

            try

            {

                var rocketEquippable = ItemCreator.CreateEquippableBuilder()

                    .CreateEquippable<RocketLauncherEquippable>("RocketLauncherEquippable")

                    .WithInteraction(canInteract: true, canPickup: true)

                    .Build();



                var akEquippable = ItemCreator.CreateEquippableBuilder()

                    .CreateEquippable<Ak47Equippable>("Ak47Equippable")

                    .WithInteraction(canInteract: true, canPickup: true)

                    .Build();



                var sniperEquippable = ItemCreator.CreateEquippableBuilder()

                    .CreateEquippable<SniperEquippable>("SniperEquippable")

                    .WithInteraction(canInteract: true, canPickup: true)

                    .Build();



                var grenadeEquippable = ItemCreator.CreateEquippableBuilder()

                    .CreateEquippable<GrenadeEquippable>("GrenadeEquippable")

                    .WithInteraction(canInteract: true, canPickup: true)

                    .Build();

                var flashbangEquippable = ItemCreator.CreateEquippableBuilder()

                    .CreateEquippable<FlashbangEquippable>("FlashbangEquippable")

                    .WithInteraction(canInteract: true, canPickup: true)

                    .Build();

                var smokeGrenadeEquippable = ItemCreator.CreateEquippableBuilder()

                    .CreateEquippable<SmokeGrenadeEquippable>("SmokeGrenadeEquippable")

                    .WithInteraction(canInteract: true, canPickup: true)

                    .Build();

                var m4a1Equippable = ItemCreator.CreateEquippableBuilder()

                    .CreateEquippable<M4a1Equippable>("M4a1Equippable")

                    .WithInteraction(canInteract: true, canPickup: true)

                    .Build();

                var drumGunEquippable = ItemCreator.CreateEquippableBuilder()

                    .CreateEquippable<DrumGunEquippable>("DrumGunEquippable")

                    .WithInteraction(canInteract: true, canPickup: true)

                    .Build();

                var nukeRemoteEquippable = ItemCreator.CreateEquippableBuilder()
                    .CreateEquippable<NukeRemoteEquippable>("NukeRemoteEquippable")
                    .WithInteraction(canInteract: true, canPickup: true)
                    .Build();

                if (S1ApiHelpers.ResolveEquippable(nukeRemoteEquippable) is NukeRemoteEquippable nukeRemotePrefab)
                    nukeRemotePrefab.PrepareRegistryPrefab();

                var magazineEquippable = ItemCreator.CreateEquippableBuilder()
                    .CreateEquippable<MagazineEquippable>("MagazineEquippable")
                    .WithInteraction(canInteract: true, canPickup: true)
                    .Build();

                _rocketLauncherDef = ItemRegistration.RegisterRocketLauncher(rocketEquippable);

                _rocketAmmoDef = ItemRegistration.RegisterRocketAmmo();

                _ak47Def = ItemRegistration.RegisterAk47(akEquippable);

                _ak47MagDef = ItemRegistration.RegisterAk47Magazine(magazineEquippable);

                _sniperDef = ItemRegistration.RegisterSniper(sniperEquippable);

                _sniperMagDef = ItemRegistration.RegisterSniperMagazine(magazineEquippable);

                _grenadeDef = ItemRegistration.RegisterGrenade(grenadeEquippable);

                _flashbangDef = ItemRegistration.RegisterFlashbang(flashbangEquippable);

                _smokeGrenadeDef = ItemRegistration.RegisterSmokeGrenade(smokeGrenadeEquippable);

                _m4a1Def = ItemRegistration.RegisterM4a1(m4a1Equippable);

                _m4a1MagDef = ItemRegistration.RegisterM4a1Magazine(magazineEquippable);

                _drumGunDef = ItemRegistration.RegisterDrumGun(drumGunEquippable);

                _drumGunMagDef = ItemRegistration.RegisterDrumGunMagazine(magazineEquippable);

                _nukeRemoteDef = ItemRegistration.RegisterNukeRemote(nukeRemoteEquippable);

                if (_grenadeDef == null)
                    MelonLogger.Error("Grenade registration failed.");



                if (_rocketLauncherDef == null)

                    MelonLogger.Error("Rocket launcher registration failed.");



                MelonLogger.Msg($"Registered MoreWeapons items.");

            }

            catch (System.Exception ex)

            {

                MelonLogger.Error($"RegisterItems failed: {ex}");

            }

        }



        private static void OnGameLoaded()
        {
            AddToShop(_drumGunDef, "Drum gun");
            AddToShop(_drumGunMagDef, "Drum");
            AddToShop(_flashbangDef, "Flashbangs");
            AddToShop(_smokeGrenadeDef, "Smoke grenades");
            AddToShop(_grenadeDef, "Grenades");
            AddToShop(_ak47Def, "AK-47");
            AddToShop(_ak47MagDef, "AK-47 magazines");
            AddToShop(_m4a1Def, "M4A1");
            AddToShop(_m4a1MagDef, "M4A1 magazines");
            AddToShop(_sniperDef, "Sniper rifle");
            AddToShop(_sniperMagDef, "Sniper magazines");
            AddToShop(_rocketLauncherDef, "Rocket launcher");
            AddToShop(_rocketAmmoDef, "Rockets");
            AddToShop(_nukeRemoteDef, "N.D.T.");

            MelonLogger.Msg(
                $"Give commands: {RocketLauncherItemId}, {RocketAmmoItemId}, {Ak47ItemId}, {Ak47MagazineItemId}, {SniperItemId}, {SniperMagazineItemId}, {GrenadeItemId}, {FlashbangItemId}, {SmokeGrenadeItemId}, {M4a1ItemId}, {M4a1MagazineItemId}, {DrumGunItemId}, {DrumGunMagazineItemId}, {NdtItemId}");
        }



        private static void AddToShop(GameIntegerItemDefinition definition, string label)

        {

            if (definition == null)

            {

                MelonLogger.Warning($"{label} registration failed.");

                return;

            }



            var wrapper = S1ApiHelpers.WrapDefinition(definition);

            if (wrapper == null)

                return;



            var added = ShopManager.AddToShops(wrapper, "Arms Dealer");

            MelonLogger.Msg($"{label} added to {added} shop listing(s).");

        }



        private static void AddToShop(GameStorableItemDefinition definition, string label)

        {

            if (definition == null)

            {

                MelonLogger.Warning($"{label} registration failed.");

                return;

            }



            var wrapper = S1ApiHelpers.WrapDefinition(definition);

            if (wrapper == null)

                return;



            var added = ShopManager.AddToShops(wrapper, "Arms Dealer");

            MelonLogger.Msg($"{label} added to {added} shop listing(s).");

        }



    }

}


