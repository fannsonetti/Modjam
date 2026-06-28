#if IL2CPP
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.PlayerScripts;
#endif

using UnityEngine;
using MoreWeapons.Utils;

namespace MoreWeapons.Weapons;

public sealed class FlashbangProjectile : MonoBehaviour
{
#if IL2CPP
    public FlashbangProjectile(System.IntPtr ptr) : base(ptr) { }
#endif

    private float _fuseDuration;
    private float _spawnTime;
    private bool _detonated;

    public static FlashbangProjectile Spawn(Vector3 origin, Vector3 velocity, float fuseDuration)
    {
        var flashObject = GrenadeProjectileSetup.CreateThrownBody(
            "FlashbangProjectile",
            origin,
            "MoreWeapons.Assets.Models.flashbang.glb",
            "Assets/Models/flashbang.glb",
            new Color(0.85f, 0.82f, 0.55f),
            bounciness: 0.2f);

        var collider = flashObject.GetComponent<SphereCollider>();

        var body = flashObject.AddComponent<Rigidbody>();
        body.useGravity = true;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.velocity = velocity;
        body.angularVelocity = UnityEngine.Random.insideUnitSphere * 10f;

        var projectile = flashObject.AddComponent<FlashbangProjectile>();
        projectile.Initialize(fuseDuration);
        projectile.IgnoreOwnerCollisions(Player.Local, collider);
        return projectile;
    }

    private void Initialize(float fuseDuration)
    {
        _fuseDuration = fuseDuration;
        _spawnTime = Time.time;
    }

    private void IgnoreOwnerCollisions(Player owner, Collider flashCollider)
    {
        if (owner == null || flashCollider == null)
            return;

        foreach (var ownerCollider in owner.GetComponentsInChildren<Collider>(true))
        {
            if (ownerCollider != null && !ownerCollider.isTrigger)
                Physics.IgnoreCollision(flashCollider, ownerCollider, true);
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

        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            if (collider != null)
                collider.enabled = false;
        }

        FlashbangEffect.Burst(point);
        Destroy(gameObject);
    }
}
