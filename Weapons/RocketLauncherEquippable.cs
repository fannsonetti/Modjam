#if IL2CPP

using Il2CppScheduleOne;

using Il2CppScheduleOne.Combat;

using Il2CppScheduleOne.DevUtilities;

using Il2CppScheduleOne.Equipping;

using Il2CppScheduleOne.ItemFramework;

using Il2CppScheduleOne.Noise;

using Il2CppScheduleOne.PlayerScripts;

using Il2CppScheduleOne.Vision;

#else

using ScheduleOne;

using ScheduleOne.Combat;

using ScheduleOne.DevUtilities;

using ScheduleOne.Equipping;

using ScheduleOne.ItemFramework;

using ScheduleOne.Noise;

using ScheduleOne.PlayerScripts;

using ScheduleOne.Vision;

#endif



using UnityEngine;

using MoreWeapons.Utils;



namespace MoreWeapons.Weapons;



public sealed class RocketLauncherEquippable : PlaceholderAvatarWeaponEquippable

{

#if IL2CPP

    public RocketLauncherEquippable(System.IntPtr ptr) : base(ptr) { }

#endif



    private const float DefaultReloadDuration = 0.55f;

    private const float MuzzleForwardOffset = 0.55f;



    private static readonly PlaceholderVisuals RocketVisuals = new(

        new Vector3(0f, 0f, 0.15f),

        new Vector3(0.03f, 0.03f, 0.18f),

        new Vector3(0f, 0f, 0.22f),

        0.025f,

        0.48f,

        Color.white,

        "RocketLauncherPlaceholder",

        "RocketLauncherThirdPerson");



    private string _reloadAnimTrigger;

    private float _reloadDuration = DefaultReloadDuration;

    private bool _isReloading;

    private float _reloadElapsed;

    private bool _viewmodelLoaded;



    protected override string TemplateItemId => Core.TemplateItemId;

    protected override PlaceholderVisuals Visuals => RocketVisuals;

    protected override float InitialTimeSinceFire => Core.RocketFireCooldown;

    protected override bool KeepHandsVisibleDuringReload => _isReloading;

    protected override bool UsesFirearmReticle => true;



    protected override float GetReticleSpreadAngle() =>

        Mathf.Lerp(Core.RocketMaxReticleSpread, Core.RocketMinReticleSpread, AimAmount);



    internal override ViewmodelProfile GetViewmodelProfile() => WeaponViewmodelProfiles.RocketLauncher;

    protected override GameObject CreateViewmodel()
    {
        var model = CreateGlbViewmodel(
            "MoreWeapons.Assets.Models.rocket_launcher_unloaded.glb",
            "Assets/Models/rocket_launcher_unloaded.glb",
            "RocketLauncherViewmodel")
            ?? base.CreateViewmodel();

        if (model != null && WeaponItem != null && WeaponItem.Value > 0)
            AttachLoadedRocketViewmodel(model);

        return model;
    }

    protected override GameObject CreateThirdPersonModel(Transform parent)
    {
        var model = CreateGlbThirdPersonModel(
            parent,
            "MoreWeapons.Assets.Models.rocket_launcher_unloaded.glb",
            "Assets/Models/rocket_launcher_unloaded.glb",
            "RocketLauncherThirdPerson")
            ?? base.CreateThirdPersonModel(parent);

        if (model != null && WeaponItem != null && WeaponItem.Value > 0)
            AttachLoadedRocketThirdPerson(model);

        return model;
    }

    private static void AttachLoadedRocketViewmodel(GameObject launcher)
    {
        var rocket = WeaponGlbLoader.LoadViewmodel(
            "MoreWeapons.Assets.Models.rocket.glb",
            "Assets/Models/rocket.glb");
        if (rocket == null)
            return;

        rocket.name = "RocketLauncherLoadedRocket";
        rocket.transform.SetParent(launcher.transform, false);
        rocket.transform.localPosition = Vector3.zero;
        rocket.transform.localRotation = Quaternion.identity;
        rocket.transform.localScale = Vector3.one;
        LayerUtility.SetLayerRecursively(rocket, LayerMask.NameToLayer("Viewmodel"));
    }

    private static void AttachLoadedRocketThirdPerson(GameObject launcher)
    {
        var rocket = WeaponGlbLoader.LoadThirdPersonModel(
            "MoreWeapons.Assets.Models.rocket.glb",
            "Assets/Models/rocket.glb");
        if (rocket == null)
            return;

        rocket.name = "RocketLauncherLoadedRocketThirdPerson";
        rocket.transform.SetParent(launcher.transform, false);
        rocket.transform.localPosition = Vector3.zero;
        rocket.transform.localRotation = Quaternion.identity;
        rocket.transform.localScale = Vector3.one;
    }

    protected override void OnWeaponEquipped()

    {

        _isReloading = false;

        _reloadElapsed = 0f;

        _viewmodelLoaded = WeaponItem != null && WeaponItem.Value > 0;

        RefreshViewmodel();

        RefreshThirdPersonModel();

    }



    protected override void ApplyWeaponTemplateSettings(Equippable_RangedWeapon template)

    {

        _reloadAnimTrigger = !string.IsNullOrEmpty(template.ReloadIndividualAnimTrigger)

            ? template.ReloadIndividualAnimTrigger

            : template.ReloadEndAnimTrigger;

        _reloadDuration = template.ReloadIndividalTime > 0.1f

            ? template.ReloadIndividalTime

            : DefaultReloadDuration;

    }



    protected override void UpdateWeaponReload()

    {

        if (!_isReloading)

            return;



        _reloadElapsed += Time.deltaTime;

        if (_reloadElapsed < _reloadDuration)

            return;



        _isReloading = false;

        WeaponItem?.SetValue(1);

        UpdateRocketVisualState();

    }



    protected override void TryStartWeaponReload()

    {

        if (WeaponItem == null || WeaponItem.Value > 0 || _isReloading || !IsEquipAnimDone)

            return;



        var inventory = PlayerSingleton<PlayerInventory>.Instance;

        if (inventory.GetAmountOfItem(Core.RocketAmmoItemId) <= 0)

            return;



        inventory.RemoveAmountOfItem(Core.RocketAmmoItemId, 1);

        _isReloading = true;

        _reloadElapsed = 0f;



        if (!string.IsNullOrEmpty(_reloadAnimTrigger))

            Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(_reloadAnimTrigger);

    }



    protected override bool CanWeaponFire()

    {

        return WeaponItem != null

            && WeaponItem.Value > 0

            && !_isReloading

            && TimeSinceFire >= Core.RocketFireCooldown

            && IsEquipAnimDone

            && AimAmount >= 0.1f;

    }



    protected override void FireWeapon()

    {

        WeaponItem.ChangeValue(-1);

        TimeSinceFire = 0f;

        UpdateRocketVisualState();



        PlayFireAnimation();

        PlayerSingleton<PlayerCamera>.Instance.JoltCamera();



        var camera = PlayerSingleton<PlayerCamera>.Instance;

        var direction = camera.transform.forward;

        var playerVelocity = PlayerSingleton<PlayerMovement>.Instance.Controller.velocity;

        var muzzle = camera.transform.position

            + direction * MuzzleForwardOffset

            + playerVelocity * 0.08f;



        NoiseUtility.EmitNoise(transform.position, ENoiseType.Gunshot, 35f, Player.Local.gameObject);

        if (Player.Local.CurrentProperty == null)

            Player.Local.VisualState.ApplyState("shooting", EVisualState.DischargingWeapon, 4f);



        RocketProjectile.Spawn(muzzle, direction, Core.RocketSpeed, Core.RocketAcceleration, Core.RocketLifetime, Core.RocketExplosion);



        Player.Local.SendEquippableMessage_Networked_Vector(

            "Shoot",

            UnityEngine.Random.Range(int.MinValue, int.MaxValue),

            muzzle + direction * 40f);

    }



    private void UpdateRocketVisualState()

    {

        var loaded = WeaponItem != null && WeaponItem.Value > 0;

        if (loaded == _viewmodelLoaded)

            return;



        _viewmodelLoaded = loaded;

        RefreshViewmodel();

        RefreshThirdPersonModel();

    }

}


