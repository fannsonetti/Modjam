#if IL2CPP

using Il2CppScheduleOne;

using Il2CppScheduleOne.Combat;

using Il2CppScheduleOne.DevUtilities;

using Il2CppScheduleOne.FX;

using Il2CppScheduleOne.PlayerScripts;

#else

using ScheduleOne;

using ScheduleOne.Combat;

using ScheduleOne.DevUtilities;

using ScheduleOne.FX;

using ScheduleOne.PlayerScripts;

#endif



using UnityEngine;



namespace MoreWeapons.Utils;



internal static class NukePlayerDamage

{

    internal static void Apply(Vector3 origin, float radius, float maxDamage, float maxForce)

    {

        var player = Player.Local;

        if (player == null)

            return;



        var playerPoint = player.transform.position + Vector3.up * 1f;

        var offset = playerPoint - origin;

        var distance = offset.magnitude;

        if (distance > radius)

            return;



        var falloff = 1f - Mathf.Clamp01(distance / radius);

        if (falloff <= 0.01f)

            return;



        var direction = distance > 0.05f ? offset / distance : Vector3.up;

        var damage = maxDamage * falloff;

        var force = maxForce * falloff;



        var damageable = player.GetComponent<IDamageable>() ?? player.GetComponentInChildren<IDamageable>();

        if (damageable != null)

        {

            var impact = new Impact(

                playerPoint,

                direction,

                force,

                damage,

                EImpactType.BluntMetal,

                player.NetworkObject,

                UnityEngine.Random.Range(int.MinValue, int.MaxValue));



            damageable.SendImpact(impact);

            Singleton<FXManager>.Instance?.CreateImpactFX(impact, damageable);

        }



        var camera = PlayerSingleton<PlayerCamera>.Instance;

        camera?.StartCameraShake(Mathf.Lerp(0.25f, 1.4f, falloff), Mathf.Lerp(0.35f, 1.8f, falloff));

    }

}

