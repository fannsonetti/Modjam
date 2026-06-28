#if IL2CPP
using Il2CppScheduleOne.AvatarFramework;
using Il2CppScheduleOne.AvatarFramework.Animation;
#else
using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Animation;
#endif

using HarmonyLib;

namespace MoreWeapons.Patches;

/// <summary>
/// Large explosions (nuke, manor destruction) can trigger cowering on NPCs whose avatar
/// body/impostor is mid-teardown. Vanilla UpdateAnimationActive assumes those refs exist.
/// </summary>
[HarmonyPatch(typeof(AvatarAnimation), "UpdateAnimationActive")]
internal static class AvatarAnimationCullingGuardPatch
{
    private static readonly AccessTools.FieldRef<AvatarAnimation, Avatar> AvatarField =
        AccessTools.FieldRefAccess<AvatarAnimation, Avatar>("avatar");

    [HarmonyPrefix]
    private static bool Prefix(AvatarAnimation __instance)
    {
        if (__instance == null)
            return false;

        var avatar = AvatarField(__instance);
        if (avatar == null || avatar.BodyContainer == null || avatar.Impostor == null || __instance.animator == null)
            return false;

        return true;
    }
}
