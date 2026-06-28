#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.DevUtilities;
#else
using ScheduleOne;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using ScheduleOne.DevUtilities;
#endif

using UnityEngine;
using MoreWeapons.Utils;

namespace MoreWeapons.Weapons;

public sealed class DrumGunEquippable : MagazineHitscanWeaponEquippable
{
#if IL2CPP
    public DrumGunEquippable(System.IntPtr ptr) : base(ptr) { }
#endif

    private static readonly PlaceholderVisuals DrumVisuals = new(
        new Vector3(0f, 0f, 0.16f),
        new Vector3(0.028f, 0.028f, 0.24f),
        new Vector3(0f, 0f, 0.2f),
        0.028f,
        0.52f,
        new Color(0.55f, 0.42f, 0.18f),
        "DrumGunPlaceholder",
        "DrumGunThirdPerson");

    protected override string TemplateItemId => Core.TemplateItemId;
    protected override PlaceholderVisuals Visuals => DrumVisuals;
    protected override string MagazineItemId => Core.DrumGunMagazineItemId;

    internal override ViewmodelProfile GetViewmodelProfile() => WeaponViewmodelProfiles.DrumGun;

    protected override GameObject CreateViewmodel() =>
        CreateGlbViewmodel("MoreWeapons.Assets.Models.drumgun.glb", "Assets/Models/drumgun.glb", "DrumGunViewmodel")
        ?? base.CreateViewmodel();

    protected override GameObject CreateThirdPersonModel(Transform parent) =>
        CreateGlbThirdPersonModel(parent, "MoreWeapons.Assets.Models.drumgun.glb", "Assets/Models/drumgun.glb", "DrumGunThirdPerson")
        ?? base.CreateThirdPersonModel(parent);

    protected override void ApplyWeaponTemplateSettings(Equippable_RangedWeapon template)
    {
        MagazineSize = Core.DrumGunMagazineSize;
        FireCooldown = Core.DrumGunFireCooldown;
        Range = Core.DrumGunRange;
        RayRadius = template.RayRadius;
        MinSpread = Core.DrumGunMinSpread;
        MaxSpread = Core.DrumGunMaxSpread;
        Damage = Core.DrumGunDamage;
        ImpactForce = Core.DrumGunImpactForce;
        HeadshotMultiplier = template.HeadshotMultiplier * 0.95f;
        TracerSpeed = template.TracerSpeed;
        AccuracyChangeDuration = template.AccuracyChangeDuration * 1.15f;
        AccuracyDropPerShot = Core.DrumGunAccuracyDropPerShot;
        FireSound = template.FireSound;
        EmptySound = template.EmptySound;

        var reloadTemplate = Registry.GetItem(Core.Ak47ReloadTemplateItemId)?.Equippable as Equippable_RangedWeapon;
        ApplyMagazineReloadTemplate(reloadTemplate);
    }
}
