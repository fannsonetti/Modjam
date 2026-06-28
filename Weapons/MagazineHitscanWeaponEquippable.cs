#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.Audio;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Noise;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.Vision;
#else
using ScheduleOne;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using ScheduleOne.Noise;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
using ScheduleOne.Vision;
#endif

using System.Collections;
using UnityEngine;
using MoreWeapons.Utils;

namespace MoreWeapons.Weapons;

public abstract class MagazineHitscanWeaponEquippable : PlaceholderAvatarWeaponEquippable
{
#if IL2CPP
    protected MagazineHitscanWeaponEquippable(System.IntPtr ptr) : base(ptr) { }
#endif

    protected int MagazineSize = 30;
    protected float FireCooldown = 0.1f;
    protected float Range = 55f;
    protected float RayRadius = 0.05f;
    protected float MinSpread = 2.5f;
    protected float MaxSpread = 10f;
    protected float Damage = 40f;
    protected float ImpactForce = 250f;
    protected float HeadshotMultiplier = 1.75f;
    protected float TracerSpeed = 50f;
    protected float AccuracyChangeDuration = 0.55f;
    protected float AccuracyDropPerShot = 0.35f;
    protected float ReloadStartTime = 1.5f;
    protected float ReloadEndTime;
    protected string ReloadStartAnimTrigger = "MagazineReload";
    protected string ReloadIndividualAnimTrigger = string.Empty;
    protected string ReloadEndAnimTrigger = string.Empty;

    internal override string EditorReloadStartAnimTrigger => ReloadStartAnimTrigger;

    internal override string EditorReloadIndividualAnimTrigger => ReloadIndividualAnimTrigger;

    internal override string EditorReloadEndAnimTrigger => ReloadEndAnimTrigger;

    protected AudioSourceController FireSound;
    protected AudioSourceController EmptySound;

    private float _accuracy;
    protected bool IsReloading;
    private Coroutine _reloadRoutine;
    private GameObject _reloadMagazineProp;

    protected abstract string MagazineItemId { get; }

    protected override float InitialTimeSinceFire => FireCooldown;
    protected override bool KeepHandsVisibleDuringReload => IsReloading;
    protected override bool UsesFirearmReticle => true;

    protected override float GetReticleSpreadAngle() => GetFireSpreadAngle();

    protected float GetFireSpreadAngle() => Mathf.Lerp(MaxSpread, MinSpread, _accuracy);

    protected override float GetAimHoldFireCooldown() => FireCooldown;

    protected override void UpdateWeaponAccuracy()
    {
        var aiming = GameInput.GetButton(GameInput.ButtonCode.SecondaryClick);
        if (aiming)
            _accuracy = Mathf.MoveTowards(_accuracy, 1f, Time.deltaTime / AccuracyChangeDuration);
        else
            _accuracy = Mathf.MoveTowards(_accuracy, 0f, Time.deltaTime / AccuracyChangeDuration * 2f);

        var movementCap = Mathf.Lerp(1f, 0f,
            Mathf.Clamp01(PlayerSingleton<PlayerMovement>.Instance.Controller.velocity.magnitude / 3.25f));
        if (_accuracy > movementCap)
            _accuracy = Mathf.MoveTowards(_accuracy, movementCap, Time.deltaTime / AccuracyChangeDuration * 2f);
    }

    protected void ApplyMagazineReloadTemplate(Equippable_RangedWeapon template)
    {
        if (template == null)
            return;

        ReloadStartTime = template.ReloadStartTime;
        ReloadEndTime = template.ReloadEndTime;
        ReloadStartAnimTrigger = template.ReloadStartAnimTrigger;
        ReloadIndividualAnimTrigger = template.ReloadIndividualAnimTrigger;
        ReloadEndAnimTrigger = template.ReloadEndAnimTrigger;
    }

    public override void Unequip()
    {
        StopReloadRoutine();
        DestroyReloadMagazineProp();
        base.Unequip();
    }

    protected override void OnWeaponEquipped()
    {
        IsReloading = false;
        _accuracy = 0f;
    }

    protected override void UpdateWeaponReload() { }

    protected override void UpdateWeaponInput()
    {
        if (GameInput.IsTyping)
            return;

        if (GameInput.GetButtonDown(GameInput.ButtonCode.Reload))
            TryStartWeaponReload();

        HandlePrimaryInput();
    }

    protected virtual void HandlePrimaryInput()
    {
        if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
        {
            if (CanWeaponFire())
                FireWeapon();
            else if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
                TryEmptyClick();
        }
    }

    protected override void TryStartWeaponReload()
    {
        if (!IsReloadReady(false))
            return;

        StopReloadRoutine();
        IsReloading = true;
        _reloadRoutine = StartWeaponCoroutine(ReloadRoutine());
    }

    protected override bool CanWeaponFire()
    {
        return WeaponItem != null
            && WeaponItem.Value > 0
            && !IsReloading
            && TimeSinceFire >= FireCooldown
            && IsEquipAnimDone
            && AimAmount >= 0.1f
            && CanWeaponFireExtra();
    }

    protected virtual bool CanWeaponFireExtra() => true;

    protected virtual (string Resource, string Path)? ReloadMagazineModel =>
        MagazineGlbCatalog.TryGet(MagazineItemId, out var glb)
            ? (glb.EmbeddedResourceName, glb.RelativePathFromModDir)
            : null;

    protected override void FireWeapon()
    {
        WeaponItem.ChangeValue(-1);
        TimeSinceFire = 0f;

        PlayFireAnimation();
        FireSound?.Play();
        PlayerSingleton<PlayerCamera>.Instance.JoltCamera();

        WeaponCombatUtility.FireHitscan(GetFireSpreadAngle(), new WeaponCombatUtility.HitscanConfig
        {
            Range = Range,
            RayRadius = RayRadius,
            Damage = Damage,
            ImpactForce = ImpactForce,
            HeadshotMultiplier = HeadshotMultiplier,
            TracerSpeed = TracerSpeed
        });

        _accuracy = Mathf.Max(_accuracy - AccuracyDropPerShot, 0f);

        NoiseUtility.EmitNoise(transform.position, ENoiseType.Gunshot, 25f, Player.Local.gameObject);
        if (Player.Local.CurrentProperty == null)
            Player.Local.VisualState.ApplyState("shooting", EVisualState.DischargingWeapon, 4f);

        OnWeaponFired();
    }

    protected virtual void OnWeaponFired() { }

    protected void TryEmptyClick()
    {
        if (WeaponItem == null || WeaponItem.Value > 0 || IsReloading)
            return;

        EmptySound?.Play();

        if (IsReloadReady(false))
            TryStartWeaponReload();
    }

    protected bool IsReloadReady(bool ignoreTiming)
    {
        if (WeaponItem == null || IsReloading)
            return false;

        if (!GetMagazine(out _))
            return false;

        if (WeaponItem.Value >= MagazineSize)
            return false;

        if (!IsEquipAnimDone && !ignoreTiming)
            return false;

        return TimeSinceFire >= FireCooldown || ignoreTiming;
    }

    protected bool GetMagazine(out StorableItemInstance mag)
    {
        mag = null;
        var inventory = PlayerSingleton<PlayerInventory>.Instance;
        for (var i = 0; i < inventory.hotbarSlots.Count; i++)
        {
            var slot = inventory.hotbarSlots[i];
            if (slot.Quantity == 0 || slot.ItemInstance.ID != MagazineItemId)
                continue;

            mag = slot.ItemInstance as StorableItemInstance;
            return mag != null;
        }

        return false;
    }

    private IEnumerator ReloadRoutine()
    {
        Singleton<ViewmodelAvatar>.Instance.SetVisibility(true);

        if (!string.IsNullOrEmpty(ReloadStartAnimTrigger))
            Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(ReloadStartAnimTrigger);

        SpawnReloadMagazineProp();

        yield return new WaitForSeconds(ReloadStartTime);

        DestroyReloadMagazineProp();

        if (GetMagazine(out var mag) && mag is IntegerItemInstance magInteger)
        {
            var roundsNeeded = MagazineSize - WeaponItem.Value;
            magInteger.ChangeValue(-roundsNeeded);
            if (magInteger.Value <= 0)
                mag.ChangeQuantity(-1);

            WeaponItem.SetValue(MagazineSize);
        }

        if (!string.IsNullOrEmpty(ReloadEndAnimTrigger))
            Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(ReloadEndAnimTrigger);

        if (ReloadEndTime > 0f)
            yield return new WaitForSeconds(ReloadEndTime);

        IsReloading = false;
        _reloadRoutine = null;
        OnReloadFinished();
    }

    protected virtual void OnReloadFinished() { }

    private void StopReloadRoutine()
    {
        if (_reloadRoutine == null)
            return;

        StopWeaponCoroutine(_reloadRoutine);
        _reloadRoutine = null;
        IsReloading = false;
        DestroyReloadMagazineProp();
    }

    private void SpawnReloadMagazineProp()
    {
        DestroyReloadMagazineProp();

        var modelInfo = ReloadMagazineModel;
        if (modelInfo == null)
            return;

        var model = WeaponGlbLoader.LoadViewmodel(modelInfo.Value.Resource, modelInfo.Value.Path);
        if (model == null)
            return;

        model.name = "ReloadMagazineProp";
        model.transform.SetParent(transform, false);
        model.transform.localPosition = HandsFreeViewmodelSettings.ModelLocalPosition;
        model.transform.localRotation = Quaternion.Euler(HandsFreeViewmodelSettings.ModelLocalEulerAngles);
        model.transform.localScale = Vector3.one * HandsFreeViewmodelSettings.ModelUniformScale;
        LayerUtility.SetLayerRecursively(model, LayerMask.NameToLayer("Viewmodel"));
        _reloadMagazineProp = model;
    }

    private void DestroyReloadMagazineProp()
    {
        if (_reloadMagazineProp == null)
            return;

        UnityEngine.Object.Destroy(_reloadMagazineProp);
        _reloadMagazineProp = null;
    }
}
