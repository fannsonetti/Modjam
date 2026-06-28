#if IL2CPP

using Il2CppScheduleOne.AvatarFramework;

using Il2CppScheduleOne.AvatarFramework.Animation;

using Il2CppScheduleOne.AvatarFramework.Impostors;

using Il2CppScheduleOne.DevUtilities;

using Il2CppScheduleOne.NPCs;

using Il2CppScheduleOne.PlayerScripts;

#else

using ScheduleOne.AvatarFramework;

using ScheduleOne.AvatarFramework.Animation;

using ScheduleOne.AvatarFramework.Impostors;

using ScheduleOne.DevUtilities;

using ScheduleOne.NPCs;

using ScheduleOne.PlayerScripts;

#endif



using System.Collections.Generic;

using UnityEngine;



namespace MoreWeapons.Utils;



internal static class ScopeNpcTargeting

{

    private const float ScopeConeDegrees = 12f;

    private const float UpdateInterval = 0.15f;



    private static readonly HashSet<NPC> ForcedNpcs = new();

    private static readonly Dictionary<NPC, bool> SavedAllowCulling = new();

    private static float _nextUpdateTime;



    internal static void Update(bool scoped)

    {

        if (!scoped)

        {

            RestoreForcedNpcs();

            return;

        }



        if (Time.time < _nextUpdateTime)

            return;



        _nextUpdateTime = Time.time + UpdateInterval;



        var camera = PlayerSingleton<PlayerCamera>.Instance;

        if (camera == null)

            return;



        var ray = camera.GetMouseRay();

        foreach (var npc in NPCManager.NPCRegistry)

            ForceFullBodyIfInCone(npc, ray);

    }



    internal static void Clear() => RestoreForcedNpcs();



    private static void ForceFullBodyIfInCone(NPC npc, Ray ray)

    {

        if (npc == null || !npc.gameObject.activeInHierarchy)

            return;



        var targetPoint = GetTargetPoint(npc);

        var toNpc = targetPoint - ray.origin;

        var distance = toNpc.magnitude;

        if (distance < 0.5f || distance > Core.SniperRange)

            return;



        if (Vector3.Angle(ray.direction, toNpc.normalized) > ScopeConeDegrees)

            return;



        ForceFullBody(npc);

    }



    private static Vector3 GetTargetPoint(NPC npc)

    {

        var avatar = npc.Avatar;

        if (avatar?.BodyContainer != null)

            return avatar.BodyContainer.position + Vector3.up * 0.9f;



        return npc.transform.position + Vector3.up * 1.4f;

    }



    private static void ForceFullBody(NPC npc)

    {

        if (npc == null || ForcedNpcs.Contains(npc))

            return;



        var avatar = npc.Avatar;

        if (avatar == null)

            return;



        var animation = avatar.Animation;

        if (animation != null)

        {

            SavedAllowCulling[npc] = animation.AllowCulling;

            animation.AllowCulling = false;

        }



        avatar.SetVisible(true);



        if (avatar.BodyContainer != null)

            avatar.BodyContainer.gameObject.SetActive(true);



        if (avatar.Impostor != null)

            avatar.Impostor.DisableImpostor();



        ForcedNpcs.Add(npc);

    }



    private static void RestoreForcedNpcs()

    {

        if (ForcedNpcs.Count == 0)

            return;



        foreach (var npc in ForcedNpcs)

        {

            if (npc == null)

                continue;



            if (SavedAllowCulling.TryGetValue(npc, out var allowCulling) && npc.Avatar?.Animation != null)

                npc.Avatar.Animation.AllowCulling = allowCulling;

        }



        ForcedNpcs.Clear();

        SavedAllowCulling.Clear();

        _nextUpdateTime = 0f;

    }

}


