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

public sealed class GrenadeEquippable : CukeThrowableEquippable
{
#if IL2CPP
    public GrenadeEquippable(System.IntPtr ptr) : base(ptr) { }
#endif

    internal override ViewmodelProfile GetProfile() => WeaponViewmodelProfiles.Grenade;

    protected override GameObject CreateViewmodel() =>
        CreateGlbViewmodel("MoreWeapons.Assets.Models.grenade.glb", "Assets/Models/grenade.glb", "GrenadeViewmodel")
        ?? CreateFallbackBar("GrenadePlaceholder", new Vector3(0.05f, 0.05f, 0.05f), new Color(0.3f, 0.5f, 0.25f));

    protected override void Throw(float throwSpeed)
    {
        if (StorableItem == null || StorableItem.Quantity <= 0)
            return;

        TimeSinceThrow = 0f;

        var camera = PlayerSingleton<PlayerCamera>.Instance;
        var origin = ThrowableWeaponUtility.GetThrowOrigin(camera);
        var velocity = ThrowableWeaponUtility.BuildThrowVelocity(camera, throwSpeed);

        GrenadeProjectile.Spawn(origin, velocity, Core.GrenadeFuseDuration, Core.GrenadeExplosion);
        NoiseUtility.EmitNoise(origin, ENoiseType.Gunshot, 18f, Player.Local.gameObject);

        ConsumeOne();
    }
}
