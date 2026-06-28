#if IL2CPP
using Il2CppFishNet;
using Il2CppScheduleOne.Combat;
using Il2CppScheduleOne.Vehicles;
#else
using FishNet;
using ScheduleOne.Combat;
using ScheduleOne.Vehicles;
#endif

using System.Collections.Generic;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ExplosionVehiclePush
{
    public static void Apply(Vector3 origin, ExplosionData data)
    {
        if (!InstanceFinder.IsServer)
            return;

        var radius = Mathf.Max(data.PushForceRadius, data.DamageRadius);
        var hits = Physics.OverlapSphere(origin, radius);
        var pushed = new HashSet<LandVehicle>();

        foreach (var hit in hits)
        {
            var vehicle = hit.GetComponentInParent<LandVehicle>();
            if (vehicle == null || !pushed.Add(vehicle))
                continue;

            PushVehicle(vehicle, origin, radius, data.MaxPushForce);
        }
    }

    private static void PushVehicle(LandVehicle vehicle, Vector3 origin, float radius, float maxPushForce)
    {
        if (vehicle.isParked)
            return;

        var body = vehicle.Rb;
        if (body == null || body.isKinematic)
            return;

        var closest = vehicle.boundingBox != null
            ? vehicle.boundingBox.ClosestPoint(origin)
            : vehicle.transform.position;
        var offset = closest - origin;
        var distance = offset.magnitude;
        if (distance > radius)
            return;

        var direction = distance > 0.05f ? offset / distance : Vector3.up;
        var falloff = 1f - Mathf.Clamp01(distance / radius);
        var targetSpeed = Mathf.Lerp(3f, 12f, falloff) * (maxPushForce / 900f);
        var impulse = body.mass * targetSpeed;

        body.WakeUp();
        body.AddForceAtPosition(direction * impulse, closest, ForceMode.Impulse);
        body.AddTorque(Vector3.Cross(Vector3.up, direction) * impulse * 0.035f, ForceMode.Impulse);
    }
}
