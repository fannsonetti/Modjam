using UnityEngine;
using MoreWeapons.Utils;

namespace MoreWeapons.Weapons;

public sealed class NukeBomberPlane : MonoBehaviour
{
#if IL2CPP
    public NukeBomberPlane(System.IntPtr ptr) : base(ptr) { }
#endif

    private Vector3 _target;
    private Vector3 _flightDirection;
    private bool _dealDamage;
    private bool _dropped;
    private float _flyHeight;
    private float _flySpeed;
    private GameObject _payload;

    internal void BeginStrike(Vector3 groundTarget, bool dealDamage)
    {
        _target = groundTarget;
        _dealDamage = dealDamage;
        _flyHeight = Core.NukePlaneAltitude;
        _flySpeed = Core.NukePlaneSpeed;

        _flightDirection = new Vector3(1f, 0f, 0.35f).normalized;

        var spawn = new Vector3(_target.x, _flyHeight, _target.z) - _flightDirection * Core.NukePlaneApproachDistance;
        transform.position = spawn;
        transform.rotation = Quaternion.LookRotation(_flightDirection, Vector3.up);

        BuildPlaneVisual();
        AttachPayload();
    }

    private void Update()
    {
        transform.position += _flightDirection * (_flySpeed * Time.deltaTime);

        if (!_dropped)
            TryDropPayload();

        var horizontalOffset = new Vector3(
            transform.position.x - _target.x,
            0f,
            transform.position.z - _target.z);

        if (_dropped && horizontalOffset.magnitude >= Core.NukePlaneExitDistance)
            Destroy(gameObject);
    }

    private void TryDropPayload()
    {
        if (_payload == null)
            return;

        var dropOrigin = _payload.transform.position;
        var dropVelocity = _flightDirection * _flySpeed;

        if (!NukeBallistics.TryPredictImpact(dropOrigin, dropVelocity, out var predictedImpact))
            return;

        var missDistance = NukeBallistics.HorizontalMissDistance(predictedImpact, _target);
        var toTarget = _target - transform.position;
        toTarget.y = 0f;
        var ahead = Vector3.Dot(toTarget, _flightDirection);
        var horizontalDist = toTarget.magnitude;

        var shouldDrop = missDistance <= Core.NukePlaneDropTolerance
            || (horizontalDist <= Core.NukePlaneDropTolerance && ahead >= 0f);

        if (!shouldDrop)
            return;

        if (ahead < -Core.NukePlaneDropTolerance)
            return;

        DropPayload(dropOrigin, dropVelocity);
    }

    private void DropPayload(Vector3 dropOrigin, Vector3 dropVelocity)
    {
        _dropped = true;

        if (_payload != null)
            Destroy(_payload);

        NukeProjectile.Drop(dropOrigin, dropVelocity, _target, _dealDamage);
    }

    private void BuildPlaneVisual()
    {
        if (WeaponGlbLoader.TryAttachProjectileVisual(
                gameObject,
                "MoreWeapons.Assets.Models.plane.glb",
                "Assets/Models/plane.glb",
                new Vector3(0.55f, 0.55f, 0.55f),
                new Vector3(0f, 270f, 0f)))
            return;

        ProceduralVisualFactory.CreateEllipsoid(
            "Fuselage",
            transform,
            Vector3.zero,
            Quaternion.Euler(0f, 0f, 90f),
            new Vector3(0.45f, 2.4f, 0.55f),
            new Color(0.42f, 0.44f, 0.48f),
            gameObject.layer);

        ProceduralVisualFactory.CreateWing(
            "LeftWing",
            transform,
            new Vector3(0f, 0f, -0.7f),
            Quaternion.identity,
            new Vector3(1.35f, 1f, 1.9f),
            new Color(0.38f, 0.4f, 0.44f),
            gameObject.layer);

        ProceduralVisualFactory.CreateWing(
            "RightWing",
            transform,
            new Vector3(0f, 0f, 0.7f),
            Quaternion.Euler(0f, 180f, 0f),
            new Vector3(1.35f, 1f, 1.9f),
            new Color(0.38f, 0.4f, 0.44f),
            gameObject.layer);

        ProceduralVisualFactory.CreateTailFin(
            "TailFin",
            transform,
            new Vector3(-1.05f, 0.28f, 0f),
            Quaternion.Euler(0f, 90f, 0f),
            new Vector3(0.7f, 1f, 0.65f),
            new Color(0.36f, 0.38f, 0.42f),
            gameObject.layer);
    }

    private void AttachPayload()
    {
        _payload = new GameObject("NukePayload");
        _payload.transform.SetParent(transform, false);
        _payload.transform.localPosition = new Vector3(0f, -0.55f, 0f);

        ProceduralVisualFactory.CreateEllipsoid(
            "NukePayloadBody",
            _payload.transform,
            Vector3.zero,
            Quaternion.identity,
            new Vector3(0.7f, 1.2f, 0.7f),
            new Color(0.35f, 0.35f, 0.35f),
            gameObject.layer);
    }
}
