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

public sealed class RocketProjectile : MonoBehaviour
{
#if IL2CPP
    public RocketProjectile(System.IntPtr ptr) : base(ptr) { }
#endif

    private const float OwnerIgnoreDuration = 0.5f;
    private static readonly Vector3 RocketScale = Vector3.one;
    private static readonly Vector3 RocketVisualEuler = new(0f, 180f, 0f);

    private float _maxLifetime;
    private float _spawnTime;
    private float _maxSpeed;
    private float _acceleration;
    private float _currentSpeed;
    private float _wobblePhase;
    private bool _exploded;
    private float _visualShakePhase;
    private Transform _visualRoot;
    private Vector3 _initialDirection;
    private Vector3 _flightDirection;
    private ExplosionData _explosionData;

    public static RocketProjectile Spawn(Vector3 origin, Vector3 direction, float maxSpeed, float acceleration, float maxLifetime, ExplosionData explosionData)
    {
        var rocketObject = new GameObject("RocketProjectile");
        rocketObject.name = "RocketProjectile";
        rocketObject.transform.position = origin;
        rocketObject.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        var collider = rocketObject.AddComponent<CapsuleCollider>();
        collider.radius = 0.12f;
        collider.height = 0.52f;
        collider.direction = 2;
        collider.material.bounciness = 0f;

        if (!WeaponGlbLoader.TryAttachProjectileVisual(
                rocketObject,
                "MoreWeapons.Assets.Models.rocket.glb",
                "Assets/Models/rocket.glb",
                Vector3.one,
                RocketVisualEuler))
        {
            ProceduralVisualFactory.CreateEllipsoid(
                "RocketProjectileFallback",
                rocketObject.transform,
                Vector3.zero,
                Quaternion.identity,
                RocketScale,
                new Color(0.85f, 0.35f, 0.1f),
                rocketObject.layer);
        }
        else
        {
            collider.enabled = false;
            var fallbackCollider = rocketObject.AddComponent<CapsuleCollider>();
            fallbackCollider.radius = 0.12f;
            fallbackCollider.height = 0.52f;
            fallbackCollider.direction = 2;
            collider = fallbackCollider;
        }

        var body = rocketObject.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        var projectile = rocketObject.AddComponent<RocketProjectile>();
        projectile.Launch(direction.normalized, maxSpeed, acceleration, maxLifetime, explosionData);
        rocketObject.AddComponent<RocketTrailEffect>();
        projectile.IgnoreOwnerCollisions(Player.Local, collider);
        return projectile;
    }

    public void Launch(Vector3 direction, float maxSpeed, float acceleration, float maxLifetime, ExplosionData explosionData)
    {
        _initialDirection = direction.normalized;
        _flightDirection = _initialDirection;
        _maxSpeed = maxSpeed;
        _acceleration = acceleration;
        _currentSpeed = Core.RocketInitialSpeed;
        _maxLifetime = maxLifetime;
        _explosionData = explosionData;
        _spawnTime = Time.time;
        _wobblePhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        _visualShakePhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        _visualRoot = transform.childCount > 0 ? transform.GetChild(0) : transform;

        var body = GetComponent<Rigidbody>();
        body.velocity = _flightDirection * _currentSpeed;
        transform.rotation = Quaternion.LookRotation(_flightDirection, Vector3.up);
    }

    private void IgnoreOwnerCollisions(Player owner, Collider rocketCollider)
    {
        if (owner == null || rocketCollider == null)
            return;

        foreach (var ownerCollider in owner.GetComponentsInChildren<Collider>(true))
        {
            if (ownerCollider != null && !ownerCollider.isTrigger)
                Physics.IgnoreCollision(rocketCollider, ownerCollider, true);
        }
    }

    private void FixedUpdate()
    {
        if (_exploded)
            return;

        _wobblePhase += Time.fixedDeltaTime * 3.5f;
        var wobble = Quaternion.AngleAxis(Mathf.Sin(_wobblePhase) * 1.75f, Vector3.up)
            * Quaternion.AngleAxis(Mathf.Cos(_wobblePhase * 0.8f) * 0.9f, Vector3.right);
        _flightDirection = (wobble * _initialDirection).normalized;

        var body = GetComponent<Rigidbody>();
        if (body != null)
        {
            if (Time.time - _spawnTime >= Core.RocketMotorDelay)
                _currentSpeed = Mathf.Min(_maxSpeed, _currentSpeed + _acceleration * Time.fixedDeltaTime);

            body.velocity = _flightDirection * _currentSpeed;
        }
    }

    private void Update()
    {
        if (_exploded)
            return;

        transform.rotation = Quaternion.LookRotation(_flightDirection, Vector3.up);
        ApplyVisualShake();

        if (Time.time - _spawnTime >= _maxLifetime)
            Explode(transform.position);
    }

    private void ApplyVisualShake()
    {
        if (_visualRoot == null)
            return;

        _visualShakePhase += Time.deltaTime * 23f;
        _visualRoot.localPosition = new Vector3(
            Mathf.Sin(_visualShakePhase * 1.25f) * 0.014f,
            Mathf.Cos(_visualShakePhase * 1.65f) * 0.01f,
            Mathf.Sin(_visualShakePhase * 0.85f) * 0.006f);
        _visualRoot.localRotation = Quaternion.Euler(RocketVisualEuler)
            * Quaternion.Euler(
                Mathf.Sin(_visualShakePhase * 1.1f) * 1.4f,
                0f,
                Mathf.Cos(_visualShakePhase * 1.35f) * 1.1f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_exploded || collision == null || collision.contactCount == 0)
            return;

        if (Time.time - _spawnTime < OwnerIgnoreDuration)
        {
            var player = collision.collider.GetComponentInParent<Player>();
            if (player != null && player == Player.Local)
                return;
        }

        Explode(collision.GetContact(0).point);
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
