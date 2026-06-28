#if IL2CPP
using Il2CppScheduleOne.Combat;
using Il2CppScheduleOne.DevUtilities;
#else
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
#endif

using UnityEngine;
using MoreWeapons.Utils;

namespace MoreWeapons.Weapons;

public sealed class NukeProjectile : MonoBehaviour
{
#if IL2CPP
    public NukeProjectile(System.IntPtr ptr) : base(ptr) { }
#endif

    private bool _detonated;
    private bool _dealDamage;
    private Vector3 _velocity;
    private Vector3 _targetGround;

    public static void Drop(
        Vector3 dropOrigin,
        Vector3 initialVelocity,
        Vector3 targetGroundPoint,
        bool dealDamage)
    {
        var nukeObject = new GameObject("NukeProjectile");
        nukeObject.transform.position = dropOrigin;

        var collider = nukeObject.AddComponent<CapsuleCollider>();
        collider.isTrigger = true;
        collider.radius = 0.55f;
        collider.height = 2.1f;
        collider.direction = 1;

        ProceduralVisualFactory.CreateEllipsoid(
            "NukeProjectileBody",
            nukeObject.transform,
            Vector3.zero,
            Quaternion.identity,
            new Vector3(1.05f, 2.1f, 1.05f),
            new Color(0.35f, 0.35f, 0.35f),
            nukeObject.layer);

        var projectile = nukeObject.AddComponent<NukeProjectile>();
        projectile.Initialize(dropOrigin, initialVelocity, targetGroundPoint, dealDamage);
    }

    private void Initialize(
        Vector3 dropOrigin,
        Vector3 initialVelocity,
        Vector3 targetGroundPoint,
        bool dealDamage)
    {
        _dealDamage = dealDamage;
        _velocity = initialVelocity;
        _targetGround = new Vector3(targetGroundPoint.x, 0f, targetGroundPoint.z);
        transform.position = dropOrigin;

        if (_velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(_velocity.normalized, Vector3.up);
    }

    private void Update()
    {
        if (_detonated)
            return;

        var deltaTime = Time.deltaTime;
        var position = transform.position;
        NukeBallistics.IntegrateStep(ref position, ref _velocity, deltaTime);
        transform.position = position;

        if (_velocity.sqrMagnitude > 0.25f)
            transform.rotation = Quaternion.LookRotation(_velocity.normalized, Vector3.up);

        if (TryGetGroundHeight(out var groundY))
        {
            if (transform.position.y <= groundY + 0.85f)
            {
                Detonate(new Vector3(_targetGround.x, groundY, _targetGround.z));
                return;
            }
        }
        else if (transform.position.y <= -10f)
        {
            Detonate(_targetGround);
        }
    }

    private bool TryGetGroundHeight(out float groundY)
    {
        if (Physics.Raycast(
                transform.position + Vector3.up * 2f,
                Vector3.down,
                out var hit,
                500f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            groundY = hit.point.y;
            return true;
        }

        groundY = 0f;
        return transform.position.y <= 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_detonated || other == null || other.isTrigger)
            return;

        Detonate(new Vector3(_targetGround.x, transform.position.y, _targetGround.z));
    }

    private void Detonate(Vector3 point)
    {
        if (_detonated)
            return;

        _detonated = true;
        var groundPoint = new Vector3(_targetGround.x, Mathf.Max(point.y, 0f), _targetGround.z);

        NukeBlastVfx.Spawn(groundPoint, Core.NukeBlastVisualRadius * Core.NukeVisualScale);

        NukeManorDetonation.TryTriggerManorExplosion(groundPoint);

        if (_dealDamage)
        {
            NukeCharredAvatar.ApplyInBlastZone(groundPoint, Core.NukeBlastRadius);

            var combatManager = NetworkSingleton<CombatManager>.Instance;
            if (combatManager != null)
                combatManager.CreateExplosion(groundPoint, Core.NukeExplosion);

            ExplosionVehiclePush.Apply(groundPoint, Core.NukeExplosion);
            NukePlayerDamage.Apply(groundPoint, Core.NukeBlastRadius, Core.NukeMaxDamage, Core.NukeMaxPushForce);
            NukeNpcEffects.Apply(groundPoint, Core.NukeBlastRadius, Core.NukeMaxDamage, Core.NukeMaxPushForce);
        }

        MushroomCloudEffect.Spawn(groundPoint, Core.NukeCloudDuration, Core.NukeCloudRadius * Core.NukeVisualScale);
        Destroy(gameObject);
    }
}
