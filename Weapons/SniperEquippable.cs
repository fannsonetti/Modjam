#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Noise;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using ScheduleOne.Noise;
using ScheduleOne.PlayerScripts;
#endif

using System.Collections;
using UnityEngine;
using MoreWeapons.Utils;

namespace MoreWeapons.Weapons;

public sealed class SniperEquippable : MagazineHitscanWeaponEquippable
{
#if IL2CPP
    public SniperEquippable(System.IntPtr ptr) : base(ptr) { }
#endif

    private static readonly PlaceholderVisuals SniperVisuals = new(
        new Vector3(0f, 0f, 0.2f),
        new Vector3(0.02f, 0.02f, 0.32f),
        new Vector3(0f, 0f, 0.26f),
        0.02f,
        0.72f,
        new Color(0.35f, 0.38f, 0.42f),
        "SniperPlaceholder",
        "SniperThirdPerson");

    private bool _isCocked;
    private bool _isCocking;
    private string _cockAnimTrigger = string.Empty;
    private float _cockTime = 0.55f;
    private Equippable_RangedWeapon _cockSoundTemplate;
    private Coroutine _cockRoutine;

    protected override string TemplateItemId => Core.TemplateItemId;
    protected override PlaceholderVisuals Visuals => SniperVisuals;
    protected override string MagazineItemId => Core.SniperMagazineItemId;
    protected override float? GetScopedFov() => Core.SniperScopedFov;

    internal override ViewmodelProfile GetViewmodelProfile() => WeaponViewmodelProfiles.Sniper;

    protected override GameObject CreateViewmodel() =>
        CreateGlbViewmodel("MoreWeapons.Assets.Models.sniper.glb", "Assets/Models/sniper.glb", "SniperViewmodel")
        ?? base.CreateViewmodel();

    protected override GameObject CreateThirdPersonModel(Transform parent) =>
        CreateGlbThirdPersonModel(parent, "MoreWeapons.Assets.Models.sniper.glb", "Assets/Models/sniper.glb", "SniperThirdPerson")
        ?? base.CreateThirdPersonModel(parent);

    protected override void ApplyWeaponTemplateSettings(Equippable_RangedWeapon template)
    {
        MagazineSize = Core.SniperMagazineSize;
        FireCooldown = Core.SniperFireCooldown;
        Range = Core.SniperRange;
        RayRadius = template.RayRadius * 0.5f;
        MinSpread = Core.SniperMinSpread;
        MaxSpread = Core.SniperMaxSpread;
        Damage = Core.SniperDamage;
        ImpactForce = Core.SniperImpactForce;
        HeadshotMultiplier = Core.SniperHeadshotMultiplier;
        TracerSpeed = template.TracerSpeed * 1.5f;
        AccuracyChangeDuration = template.AccuracyChangeDuration;
        AccuracyDropPerShot = 0.03f;
        FireSound = template.FireSound;
        EmptySound = template.EmptySound;
        _cockTime = template.CockTime;
        _cockSoundTemplate = template;

        var reloadTemplate = Registry.GetItem(Core.Ak47ReloadTemplateItemId)?.Equippable as Equippable_RangedWeapon;
        ApplyMagazineReloadTemplate(reloadTemplate);
    }

    protected override void OnWeaponEquipped()
    {
        base.OnWeaponEquipped();
        _isCocked = false;
        _isCocking = false;
    }

    public override void Unequip()
    {
        StopCockRoutine();
        ScopeOverlay.Hide();
        ScopeHudHelper.Reset();
        ScopeMouseSensitivity.Reset();
        ScopeNpcTargeting.Clear();
        base.Unequip();
    }

    protected override void OnStopAim()
    {
        ScopeOverlay.Hide();
        ScopeHudHelper.Reset();
        ScopeMouseSensitivity.Reset();
        ScopeNpcTargeting.Clear();
    }

    protected override void OnAimAmountChanged(float aim)
    {
        var holdingScope = GameInput.GetButton(GameInput.ButtonCode.SecondaryClick);
        var scoped = holdingScope && aim > 0.5f;
        ScopeOverlay.SetVisible(scoped);
        ScopeHudHelper.SetScoped(scoped);
        ScopeMouseSensitivity.SetScoped(scoped, holdingScope ? aim : 0f);
        ScopeNpcTargeting.Update(scoped);
    }

    protected override bool CanWeaponFireExtra() => _isCocked && !_isCocking;

    protected override void HandlePrimaryInput()
    {
        if (!GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
            return;

        if (CanWeaponFire())
        {
            FireWeapon();
            return;
        }

        if (WeaponItem != null && WeaponItem.Value > 0 && !_isCocked && !_isCocking && !IsReloading && IsEquipAnimDone)
            StartCock();
        else if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
            TryEmptyClick();
    }

    protected override void OnWeaponFired()
    {
        _isCocked = false;
    }

    protected override void OnReloadFinished()
    {
        _isCocked = false;
    }

    private void StartCock()
    {
        StopCockRoutine();
        _isCocking = true;
        _cockRoutine = StartWeaponCoroutine(CockRoutine());
    }

    private IEnumerator CockRoutine()
    {
        WeaponAudioUtility.PlayPumpCockSound(_cockSoundTemplate, 2.5f);

        if (!string.IsNullOrEmpty(_cockAnimTrigger))
            Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(_cockAnimTrigger);

        yield return new WaitForSeconds(_cockTime);
        _isCocked = true;
        _isCocking = false;
        _cockRoutine = null;
    }

    private void StopCockRoutine()
    {
        if (_cockRoutine == null)
            return;

        StopWeaponCoroutine(_cockRoutine);
        _cockRoutine = null;
        _isCocking = false;
    }
}
