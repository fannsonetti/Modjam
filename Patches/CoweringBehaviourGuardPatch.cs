#if IL2CPP
using Il2CppScheduleOne.NPCs.Behaviour;
#else
using ScheduleOne.NPCs.Behaviour;
#endif

using HarmonyLib;

namespace MoreWeapons.Patches;

[HarmonyPatch(typeof(CoweringBehaviour), "SetCowering")]
internal static class CoweringBehaviourGuardPatch
{
    [HarmonyPrefix]
    private static bool Prefix(CoweringBehaviour __instance)
    {
        var npc = __instance?.Npc;
        if (npc == null)
            return false;

        var avatar = npc.Avatar;
        if (avatar == null)
            return false;

        var animation = avatar.Animation;
        if (animation == null || animation.animator == null)
            return false;

        if (avatar.BodyContainer == null || avatar.Impostor == null)
            return false;

        return true;
    }
}
