#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Noise;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne;
using ScheduleOne.DevUtilities;
using ScheduleOne.Noise;
using ScheduleOne.PlayerScripts;
#endif

using UnityEngine;
using MoreWeapons.Utils;

namespace MoreWeapons.Weapons;

public sealed class SmokeGrenadeEquippable : CukeThrowableEquippable
{
#if IL2CPP
    public SmokeGrenadeEquippable(System.IntPtr ptr) : base(ptr) { }
#endif

    internal override ViewmodelProfile GetProfile() => WeaponViewmodelProfiles.SmokeGrenade;

    protected override GameObject CreateViewmodel() =>
        CreateGlbViewmodel("MoreWeapons.Assets.Models.smoke_grenade.glb", "Assets/Models/smoke_grenade.glb", "SmokeGrenadeViewmodel")
        ?? CreateFallbackBar("SmokeGrenadePlaceholder", new Vector3(0.05f, 0.05f, 0.05f), new Color(0.55f, 0.55f, 0.55f));

    protected override void Throw(float throwSpeed)
    {
        if (StorableItem == null || StorableItem.Quantity <= 0)
            return;

        TimeSinceThrow = 0f;

        var camera = PlayerSingleton<PlayerCamera>.Instance;
        var origin = ThrowableWeaponUtility.GetThrowOrigin(camera);
        var velocity = ThrowableWeaponUtility.BuildThrowVelocity(camera, throwSpeed);

        SmokeGrenadeProjectile.Spawn(origin, velocity, Core.SmokeGrenadeFuseDuration);
        NoiseUtility.EmitNoise(origin, ENoiseType.Gunshot, 10f, Player.Local.gameObject);

        ConsumeOne();
    }
}
