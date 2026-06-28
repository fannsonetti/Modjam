using UnityEngine;

namespace MoreWeapons.Utils;

internal static class NukeBallistics
{
    private const float SimStep = 0.05f;
    private const int MaxSteps = 600;

    internal static float Gravity => Core.NukeProjectileGravity;
    internal static float HorizontalDrag => Core.NukeProjectileHorizontalDrag;

    internal static bool TryPredictImpact(
        Vector3 origin,
        Vector3 initialVelocity,
        out Vector3 impactPoint)
    {
        if (!Simulate(origin, initialVelocity, out var endPosition, out _))
        {
            impactPoint = default;
            return false;
        }

        impactPoint = new Vector3(endPosition.x, 0f, endPosition.z);
        return true;
    }

    internal static float HorizontalMissDistance(Vector3 impactPoint, Vector3 targetGroundPoint)
    {
        var delta = impactPoint - targetGroundPoint;
        delta.y = 0f;
        return delta.magnitude;
    }

    internal static void IntegrateStep(ref Vector3 position, ref Vector3 velocity, float deltaTime)
    {
        velocity.y -= Gravity * deltaTime;

        var horizontal = new Vector3(velocity.x, 0f, velocity.z);
        var dragFactor = Mathf.Exp(-HorizontalDrag * deltaTime);
        horizontal *= dragFactor;
        velocity.x = horizontal.x;
        velocity.z = horizontal.z;

        position += velocity * deltaTime;
    }

    private static bool Simulate(
        Vector3 origin,
        Vector3 initialVelocity,
        out Vector3 endPosition,
        out Vector3 endVelocity)
    {
        var position = origin;
        var velocity = initialVelocity;

        for (var step = 0; step < MaxSteps; step++)
        {
            if (position.y <= 0.75f)
            {
                endPosition = position;
                endVelocity = velocity;
                return true;
            }

            IntegrateStep(ref position, ref velocity, SimStep);
        }

        endPosition = position;
        endVelocity = velocity;
        return position.y <= origin.y;
    }
}
