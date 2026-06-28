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



public sealed class Ak47Equippable : MagazineHitscanWeaponEquippable

{

#if IL2CPP

    public Ak47Equippable(System.IntPtr ptr) : base(ptr) { }

#endif



    private static readonly PlaceholderVisuals AkVisuals = new(

        new Vector3(0f, 0f, 0.18f),

        new Vector3(0.025f, 0.025f, 0.28f),

        new Vector3(0f, 0f, 0.24f),

        0.022f,

        0.62f,

        new Color(0.75f, 0.75f, 0.75f),

        "Ak47Placeholder",

        "Ak47ThirdPerson");



    protected override string TemplateItemId => Core.TemplateItemId;

    protected override PlaceholderVisuals Visuals => AkVisuals;

    protected override string MagazineItemId => Core.Ak47MagazineItemId;

    internal override ViewmodelProfile GetViewmodelProfile() => WeaponViewmodelProfiles.Ak47;



    protected override GameObject CreateViewmodel() =>

        CreateGlbViewmodel("MoreWeapons.Assets.Models.ak47.glb", "Assets/Models/ak47.glb", "Ak47Viewmodel")

        ?? base.CreateViewmodel();



    protected override GameObject CreateThirdPersonModel(Transform parent) =>

        CreateGlbThirdPersonModel(parent, "MoreWeapons.Assets.Models.ak47.glb", "Assets/Models/ak47.glb", "Ak47ThirdPerson")

        ?? base.CreateThirdPersonModel(parent);



    protected override void ApplyWeaponTemplateSettings(Equippable_RangedWeapon template)

    {

        MagazineSize = Core.Ak47MagazineSize;

        FireCooldown = Core.Ak47FireCooldown;

        Range = Core.Ak47Range;

        RayRadius = template.RayRadius;

        MinSpread = Core.Ak47MinSpread;

        MaxSpread = Core.Ak47MaxSpread;

        Damage = Core.Ak47Damage;

        ImpactForce = Core.Ak47ImpactForce;

        HeadshotMultiplier = template.HeadshotMultiplier;

        TracerSpeed = template.TracerSpeed;

        AccuracyChangeDuration = template.AccuracyChangeDuration;

        AccuracyDropPerShot = Core.Ak47AccuracyDropPerShot;

        FireSound = template.FireSound;

        EmptySound = template.EmptySound;



        var reloadTemplate = Registry.GetItem(Core.Ak47ReloadTemplateItemId)?.Equippable as Equippable_RangedWeapon;

        ApplyMagazineReloadTemplate(reloadTemplate);

    }

}


