using UnityEngine;

namespace MoreWeapons.Utils;

internal static class WeaponViewmodelProfiles
{
    internal static ViewmodelProfile Ak47 => new()
    {
        Label = "Ak47",
        PresentationMode = ViewmodelPresentationMode.AvatarHands,
        HipPosition = new Vector3(0f, 0f, 0.12f),
        HipEuler = new Vector3(0f, 180f, 0f),
        UniformScale = 1f,
        AimEuler = new Vector3(-9f, 18f, 0f),
        AimPositionOffset = new Vector3(0f, 0f, 0.05f),
        ThirdPersonPosition = new Vector3(0f, 0f, 0.42f),
        ThirdPersonWorldScale = 0.9f,
    };

    internal static ViewmodelProfile Sniper => new()
    {
        Label = "Sniper",
        PresentationMode = ViewmodelPresentationMode.AvatarHands,
        HipPosition = new Vector3(0f, 0f, 0.14f),
        HipEuler = new Vector3(0f, 180f, 0f),
        UniformScale = 1f,
        AimEuler = new Vector3(-9f, 18f, 0f),
        AimPositionOffset = new Vector3(0f, 0f, 0.05f),
        ThirdPersonPosition = new Vector3(0f, 0f, 0.44f),
        ThirdPersonWorldScale = 1.25f,
    };

    internal static ViewmodelProfile M4a1 => new()
    {
        Label = "M4a1",
        PresentationMode = ViewmodelPresentationMode.AvatarHands,
        HipPosition = new Vector3(0f, 0f, 0.04f),
        HipEuler = new Vector3(0f, 180f, 0f),
        UniformScale = 1f,
        AimEuler = new Vector3(-9f, 18f, 0f),
        AimPositionOffset = new Vector3(-0.03f, 0f, 0.02f),
        ThirdPersonPosition = new Vector3(0f, 0f, 0.40f),
        ThirdPersonWorldScale = 0.9f,
    };

    internal static ViewmodelProfile RocketLauncher => new()
    {
        Label = "RocketLauncher",
        PresentationMode = ViewmodelPresentationMode.AvatarHands,
        HipPosition = new Vector3(0f, 0f, 0.07f),
        HipEuler = new Vector3(0f, 180f, 0f),
        UniformScale = 1f,
        AimEuler = new Vector3(-9f, 18f, 0f),
        AimPositionOffset = new Vector3(0f, 0f, 0.02f),
        ThirdPersonPosition = new Vector3(0f, 0f, 0.40f),
        ThirdPersonWorldScale = 0.95f,
    };

    internal static ViewmodelProfile DrumGun => new()
    {
        Label = "DrumGun",
        PresentationMode = ViewmodelPresentationMode.AvatarHands,
        HipPosition = new Vector3(0f, 0f, 0.04f),
        HipEuler = new Vector3(0f, 180f, 0f),
        UniformScale = 1f,
        AimEuler = new Vector3(-9f, 18f, 0f),
        AimPositionOffset = new Vector3(0f, 0f, 0.02f),
        ThirdPersonPosition = new Vector3(0f, 0f, 0.42f),
        ThirdPersonWorldScale = 0.9f,
    };

    internal static ViewmodelProfile Grenade => CreateFloatingThrowable("Grenade");
    internal static ViewmodelProfile SmokeGrenade => CreateFloatingThrowable("SmokeGrenade");
    internal static ViewmodelProfile Flashbang => CreateFloatingThrowable("Flashbang");
    internal static ViewmodelProfile Magazine => CreateFloatingThrowable("Magazine");
    internal static ViewmodelProfile Ndt => CreateFloatingNdt();

    private static ViewmodelProfile CreateFloatingThrowable(string label, float modelScale = HandsFreeViewmodelSettings.ModelUniformScale)
    {
        var profile = ViewmodelProfile.CreateFloating(label, modelScale);
        profile.ThirdPersonWorldScale = 0.4f;
        return profile;
    }

    private static ViewmodelProfile CreateFloatingNdt()
    {
        var profile = ViewmodelProfile.CreateFloating("Ndt", HandsFreeViewmodelSettings.ModelUniformScale, includeMapRaise: true);
        profile.ThirdPersonWorldScale = 0.45f;
        profile.EquippableLocalPosition = new Vector3(0.3818f, -0.2445f, 0.2328f);
        profile.EquippableLocalEuler = new Vector3(0f, 300f, 350f);
        profile.ModelLocalPosition = HandsFreeViewmodelSettings.ModelLocalPosition;
        profile.ModelLocalEuler = HandsFreeViewmodelSettings.ModelLocalEulerAngles;
        profile.MapRaiseEquippablePosition = new Vector3(0.0766f, 0.1564f, 0.0956f);
        profile.MapRaiseEquippableEuler = new Vector3(90f, 270f, 0f);
        return profile;
    }
}
