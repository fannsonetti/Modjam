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

using System.Collections.Generic;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class WeaponCombatUtility
{
    internal struct HitscanConfig
    {
        public float Range;
        public float RayRadius;
        public float Damage;
        public float ImpactForce;
        public float HeadshotMultiplier;
        public float TracerSpeed;
    }

    internal static Vector3 SpreadDirection(Vector3 direction, float maxAngle)
    {
        direction.Normalize();
        var maxRadians = maxAngle * Mathf.Deg2Rad;
        var spread = UnityEngine.Random.Range(0f, maxRadians);
        var rotation = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        var offset = new Vector3(
            Mathf.Sin(spread) * Mathf.Cos(rotation),
            Mathf.Sin(spread) * Mathf.Sin(rotation),
            Mathf.Cos(spread));
        return (Quaternion.FromToRotation(Vector3.forward, direction) * offset).normalized;
    }

    internal static void FireHitscan(float spreadAngle, HitscanConfig config)
    {
        var camera = PlayerSingleton<PlayerCamera>.Instance;
        var origin = camera.transform.position
            + camera.transform.forward * 0.4f
            + camera.transform.right * 0.1f
            + camera.transform.up * -0.03f;
        var direction = SpreadDirection(camera.transform.forward, spreadAngle);

        Singleton<FXManager>.Instance.CreateBulletTrail(
            origin,
            direction,
            config.TracerSpeed,
            config.Range,
            NetworkSingleton<CombatManager>.Instance.RangedWeaponLayerMask);

        var endPoint = origin + direction * config.Range;
        var hits = Physics.SphereCastAll(
            origin,
            config.RayRadius,
            direction,
            config.Range,
            NetworkSingleton<CombatManager>.Instance.RangedWeaponLayerMask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        var damageables = new Dictionary<IDamageable, List<RaycastHit>>();
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("CombatIgnore"))
                continue;

            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable == null || ReferenceEquals(damageable, Player.Local))
                continue;

            endPoint = hit.point;
            if (!damageables.TryGetValue(damageable, out var hitList))
            {
                hitList = new List<RaycastHit>();
                damageables[damageable] = hitList;
            }

            hitList.Add(hit);
            break;
        }

        Player.Local.SendEquippableMessage_Networked_Vector(
            "Shoot",
            UnityEngine.Random.Range(int.MinValue, int.MaxValue),
            endPoint);

        foreach (var pair in damageables)
        {
            var hitList = pair.Value;
            var hitPoint = Vector3.zero;
            foreach (var hit in hitList)
                hitPoint += hit.point;
            hitPoint /= hitList.Count;

            var damage = config.Damage;
            if (hitList[0].collider.CompareTag("Head"))
                damage *= config.HeadshotMultiplier;

            var impact = new Impact(
                hitPoint,
                camera.transform.forward,
                config.ImpactForce,
                damage,
                EImpactType.Bullet,
                Player.Local.NetworkObject,
                UnityEngine.Random.Range(int.MinValue, int.MaxValue));

            pair.Key.SendImpact(impact);
            Singleton<FXManager>.Instance.CreateImpactFX(impact, pair.Key);
        }
    }
}
