#if IL2CPP
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.PlayerScripts;
#endif

using UnityEngine;
using MoreWeapons.Utils;

namespace MoreWeapons.Weapons;

public sealed class SmokeGrenadeProjectile : MonoBehaviour
{
#if IL2CPP
    public SmokeGrenadeProjectile(System.IntPtr ptr) : base(ptr) { }
#endif

    private float _fuseDuration;
    private float _spawnTime;
    private bool _detonated;

    public static SmokeGrenadeProjectile Spawn(Vector3 origin, Vector3 velocity, float fuseDuration)
    {
        var smokeObject = GrenadeProjectileSetup.CreateThrownBody(
            "SmokeGrenadeProjectile",
            origin,
            "MoreWeapons.Assets.Models.smoke_grenade.glb",
            "Assets/Models/smoke_grenade.glb",
            new Color(0.55f, 0.55f, 0.55f),
            bounciness: 0.15f);

        var collider = smokeObject.GetComponent<SphereCollider>();

        var body = smokeObject.AddComponent<Rigidbody>();
        body.useGravity = true;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.velocity = velocity;
        body.angularVelocity = UnityEngine.Random.insideUnitSphere * 8f;

        var projectile = smokeObject.AddComponent<SmokeGrenadeProjectile>();
        projectile.Initialize(fuseDuration);
        projectile.IgnoreOwnerCollisions(Player.Local, collider);
        return projectile;
    }

    private void Initialize(float fuseDuration)
    {
        _fuseDuration = fuseDuration;
        _spawnTime = Time.time;
    }

    private void IgnoreOwnerCollisions(Player owner, Collider smokeCollider)
    {
        if (owner == null || smokeCollider == null)
            return;

        foreach (var ownerCollider in owner.GetComponentsInChildren<Collider>(true))
        {
            if (ownerCollider != null && !ownerCollider.isTrigger)
                Physics.IgnoreCollision(smokeCollider, ownerCollider, true);
        }
    }

    private void Update()
    {
        if (_detonated)
            return;

        if (Time.time - _spawnTime >= _fuseDuration)
            Detonate(transform.position);
    }

    private void Detonate(Vector3 point)
    {
        if (_detonated)
            return;

        _detonated = true;

        var body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
        }

        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            if (collider != null)
                collider.enabled = false;
        }

        SmokeCloudEffect.Spawn(point, Core.SmokeCloudDuration, Core.SmokeCloudRadius);
    }
}
