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



public sealed class M4a1Equippable : MagazineHitscanWeaponEquippable

{

#if IL2CPP

    public M4a1Equippable(System.IntPtr ptr) : base(ptr) { }

#endif



    private static readonly PlaceholderVisuals M4Visuals = new(

        new Vector3(0f, 0f, 0.17f),

        new Vector3(0.022f, 0.022f, 0.26f),

        new Vector3(0f, 0f, 0.22f),

        0.02f,

        0.58f,

        new Color(0.32f, 0.34f, 0.36f),

        "M4a1Placeholder",

        "M4a1ThirdPerson");



    protected override string TemplateItemId => Core.TemplateItemId;

    protected override PlaceholderVisuals Visuals => M4Visuals;

    protected override string MagazineItemId => Core.M4a1MagazineItemId;

    internal override ViewmodelProfile GetViewmodelProfile() => WeaponViewmodelProfiles.M4a1;



    protected override GameObject CreateViewmodel() =>

        CreateGlbViewmodel("MoreWeapons.Assets.Models.m4a1.glb", "Assets/Models/m4a1.glb", "M4a1Viewmodel")

        ?? base.CreateViewmodel();



    protected override GameObject CreateThirdPersonModel(Transform parent) =>

        CreateGlbThirdPersonModel(parent, "MoreWeapons.Assets.Models.m4a1.glb", "Assets/Models/m4a1.glb", "M4a1ThirdPerson")

        ?? base.CreateThirdPersonModel(parent);



    protected override void ApplyWeaponTemplateSettings(Equippable_RangedWeapon template)

    {

        MagazineSize = Core.M4a1MagazineSize;

        FireCooldown = Core.M4a1FireCooldown;

        Range = Core.M4a1Range;

        RayRadius = template.RayRadius * 0.9f;

        MinSpread = Core.M4a1MinSpread;

        MaxSpread = Core.M4a1MaxSpread;

        Damage = Core.M4a1Damage;

        ImpactForce = Core.M4a1ImpactForce;

        HeadshotMultiplier = template.HeadshotMultiplier;

        TracerSpeed = template.TracerSpeed;

        AccuracyChangeDuration = template.AccuracyChangeDuration * 0.85f;

        AccuracyDropPerShot = Core.M4a1AccuracyDropPerShot;

        FireSound = template.FireSound;

        EmptySound = template.EmptySound;



        var reloadTemplate = Registry.GetItem(Core.Ak47ReloadTemplateItemId)?.Equippable as Equippable_RangedWeapon;

        ApplyMagazineReloadTemplate(reloadTemplate);

    }

}


