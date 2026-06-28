#if IL2CPP
using Il2CppScheduleOne.Property;
#else
using ScheduleOne.Property;
#endif

using MelonLoader;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class NukeManorDetonation
{
    private static readonly Vector3 ManorTriggerPoint = new(165f, 11f, -60f);
    private const float ManorTriggerRadius = 20f;

    internal static void TryTriggerManorExplosion(Vector3 detonationPoint)
    {
        if (Vector3.Distance(detonationPoint, ManorTriggerPoint) > ManorTriggerRadius)
            return;

        var manor = FindManor();
        if (manor == null)
        {
            MelonLogger.Warning("MoreWeapons: nuke landed near the manor trigger but no Manor instance was found.");
            return;
        }

        manor.Explode();
        MelonLogger.Msg("MoreWeapons: nuke detonation triggered Manor.Explode().");
    }

    private static Manor FindManor()
    {
        foreach (var property in Property.Properties)
        {
            if (property == null)
                continue;

#if IL2CPP
            if (property.TryCast<Manor>() is { } il2CppManor)
                return il2CppManor;
#else
            if (property is Manor manor)
                return manor;
#endif
        }

        var candidates = Resources.FindObjectsOfTypeAll<Manor>();
        foreach (var candidate in candidates)
        {
            if (candidate == null)
                continue;

            var scene = candidate.gameObject.scene;
            if (scene.IsValid() && scene.isLoaded)
                return candidate;
        }

        return null;
    }
}
