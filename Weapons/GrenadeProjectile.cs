#if IL2CPP
using Il2CppScheduleOne.Combat;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
#endif

using UnityEngine;
using MoreWeapons.Utils;

namespace MoreWeapons.Weapons;

public sealed class GrenadeProjectile : MonoBehaviour
{
#if IL2CPP
    public GrenadeProjectile(System.IntPtr ptr) : base(ptr) { }
#endif

    private float _fuseDuration;
    private float _spawnTime;
    private bool _exploded;
    private ExplosionData _explosionData;

    public static GrenadeProjectile Spawn(Vector3 origin, Vector3 velocity, float fuseDuration, ExplosionData explosionData)
    {
        var grenadeObject = GrenadeProjectileSetup.CreateThrownBody(
            "GrenadeProjectile",
            origin,
            "MoreWeapons.Assets.Models.grenade.glb",
            "Assets/Models/grenade.glb",
            new Color(0.25f, 0.45f, 0.2f),
            bounciness: 0.15f);

        var collider = grenadeObject.GetComponent<SphereCollider>();

        var body = grenadeObject.AddComponent<Rigidbody>();
        body.useGravity = true;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.velocity = velocity;
        body.angularVelocity = UnityEngine.Random.insideUnitSphere * 8f;

        var projectile = grenadeObject.AddComponent<GrenadeProjectile>();
        projectile.Initialize(fuseDuration, explosionData);
        projectile.IgnoreOwnerCollisions(Player.Local, collider);
        return projectile;
    }

    private void Initialize(float fuseDuration, ExplosionData explosionData)
    {
        _fuseDuration = fuseDuration;
        _explosionData = explosionData;
        _spawnTime = Time.time;
    }

    private void IgnoreOwnerCollisions(Player owner, Collider grenadeCollider)
    {
        if (owner == null || grenadeCollider == null)
            return;

        foreach (var ownerCollider in owner.GetComponentsInChildren<Collider>(true))
        {
            if (ownerCollider != null && !ownerCollider.isTrigger)
                Physics.IgnoreCollision(grenadeCollider, ownerCollider, true);
        }
    }

    private void Update()
    {
        if (_exploded)
            return;

        if (Time.time - _spawnTime >= _fuseDuration)
            Explode(transform.position);
    }

    private void Explode(Vector3 point)
    {
        if (_exploded)
            return;

        _exploded = true;

        var combatManager = NetworkSingleton<CombatManager>.Instance;
        if (combatManager != null)
            combatManager.CreateExplosion(point, _explosionData);

        ExplosionVehiclePush.Apply(point, _explosionData);
        Destroy(gameObject);
    }
}
