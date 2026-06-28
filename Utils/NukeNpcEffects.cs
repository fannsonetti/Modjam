#if IL2CPP
using Il2CppScheduleOne.Combat;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.Combat;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
#endif

using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class NukeNpcEffects
{
    private const int NpcsPerFrame = 8;

    internal static void Apply(
        Vector3 origin,
        float damageRadius,
        float maxDamage,
        float maxPushForce)
    {
        MelonCoroutines.Start(ApplyRoutine(origin, damageRadius, maxDamage, maxPushForce));
    }

    private static IEnumerator ApplyRoutine(
        Vector3 origin,
        float damageRadius,
        float maxDamage,
        float maxPushForce)
    {
        var pushForceRadius = damageRadius * 2f;
        var queryRadius = Mathf.Max(damageRadius, pushForceRadius);
        var pending = new List<NPC>(32);

        foreach (var npc in NPCManager.NPCRegistry)
        {
            if (npc == null || !npc.gameObject.activeInHierarchy || npc.Health == null)
                continue;

            if (npc.Health.IsDead || npc.Health.IsKnockedOut)
                continue;

            var targetPoint = GetTargetPoint(npc);
            if ((targetPoint - origin).sqrMagnitude > queryRadius * queryRadius)
                continue;

            pending.Add(npc);
        }

        for (var i = 0; i < pending.Count; i++)
        {
            ApplyToNpc(pending[i], origin, damageRadius, pushForceRadius, maxDamage, maxPushForce);
            if ((i + 1) % NpcsPerFrame == 0)
                yield return null;
        }
    }

    private static void ApplyToNpc(
        NPC npc,
        Vector3 origin,
        float damageRadius,
        float pushForceRadius,
        float maxDamage,
        float maxPushForce)
    {
        var targetPoint = GetTargetPoint(npc);
        var offset = targetPoint - origin;
        var distance = offset.magnitude;
        var direction = distance > 0.05f ? offset / distance : Vector3.up;
        var damage = maxDamage * (1f - Mathf.Clamp01(distance / damageRadius));
        var force = maxPushForce * (1f - Mathf.Clamp01(distance / pushForceRadius));
        if (damage <= 0.01f && force <= 50f)
            return;

        var impact = new Impact(
            targetPoint,
            direction,
            force,
            damage,
            EImpactType.Explosion,
            Player.Local != null ? Player.Local.NetworkObject : null,
            UnityEngine.Random.Range(int.MinValue, int.MaxValue));

        npc.SendImpact(impact);
    }

    private static Vector3 GetTargetPoint(NPC npc)
    {
        if (npc.Avatar != null)
            return npc.Avatar.transform.position + Vector3.up * 0.9f;

        return npc.transform.position + Vector3.up * 1f;
    }
}
