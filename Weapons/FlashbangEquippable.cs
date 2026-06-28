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

public sealed class FlashbangEquippable : CukeThrowableEquippable
{
#if IL2CPP
    public FlashbangEquippable(System.IntPtr ptr) : base(ptr) { }
#endif

    internal override ViewmodelProfile GetProfile() => WeaponViewmodelProfiles.Flashbang;

    protected override GameObject CreateViewmodel() =>
        CreateGlbViewmodel("MoreWeapons.Assets.Models.flashbang.glb", "Assets/Models/flashbang.glb", "FlashbangViewmodel")
        ?? CreateFallbackBar("FlashbangPlaceholder", new Vector3(0.045f, 0.045f, 0.045f), new Color(0.82f, 0.78f, 0.45f));

    protected override void Throw(float throwSpeed)
    {
        if (StorableItem == null || StorableItem.Quantity <= 0)
            return;

        TimeSinceThrow = 0f;

        var camera = PlayerSingleton<PlayerCamera>.Instance;
        var origin = ThrowableWeaponUtility.GetThrowOrigin(camera);
        var velocity = ThrowableWeaponUtility.BuildThrowVelocity(camera, throwSpeed);

        FlashbangProjectile.Spawn(origin, velocity, Core.FlashbangFuseDuration);
        NoiseUtility.EmitNoise(origin, ENoiseType.Gunshot, 12f, Player.Local.gameObject);

        ConsumeOne();
    }
}
