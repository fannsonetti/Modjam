#if IL2CPP
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
#endif

using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ThrowableWeaponUtility
{
    internal const float PrimaryThrowSpeed = 15f;
    internal const float SoftThrowSpeed = 8f;
    private const float PlayerVelocityInheritance = 0.35f;

    internal static Vector3 GetThrowOrigin(PlayerCamera camera)
    {
        return camera.transform.position
            + camera.transform.forward * 0.55f
            + camera.transform.up * -0.05f
            + camera.transform.right * 0.12f;
    }

    internal static Vector3 GetLookDirection(PlayerCamera camera) =>
        camera.transform.forward.normalized;

    internal static Vector3 CalculateForwardThrowVelocity(Vector3 lookDirection, float throwSpeed)
    {
        if (lookDirection.sqrMagnitude < 0.001f)
            return Vector3.forward * throwSpeed;

        return lookDirection.normalized * throwSpeed;
    }

    internal static Vector3 BuildThrowVelocity(PlayerCamera camera, float throwSpeed)
    {
        var lookDirection = GetLookDirection(camera);
        var velocity = CalculateForwardThrowVelocity(lookDirection, throwSpeed);
        var movement = PlayerSingleton<PlayerMovement>.Instance;
        if (movement?.Controller != null)
            velocity += movement.Controller.velocity * PlayerVelocityInheritance;

        return velocity;
    }
}
