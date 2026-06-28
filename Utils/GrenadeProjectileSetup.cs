using UnityEngine;

namespace MoreWeapons.Utils;

internal static class GrenadeProjectileSetup
{
    internal const float ThrownVisualScale = 1.5f;

    /// <summary>Tight collider matched to the thrown GLB visual.</summary>
    internal const float ColliderRadius = 0.08f;

    internal static readonly Vector3 ThrownVisualScaleVector = Vector3.one * ThrownVisualScale;

    internal static GameObject CreateThrownBody(
        string objectName,
        Vector3 origin,
        string embeddedResourceName,
        string relativePathFromModDir,
        Color fallbackColor,
        float bounciness)
    {
        var root = new GameObject(objectName);
        root.name = objectName;
        root.transform.position = origin;
        root.transform.localScale = Vector3.one;

        var collider = root.AddComponent<SphereCollider>();
        collider.radius = ColliderRadius;
        collider.material.bounciness = bounciness;

        if (!WeaponGlbLoader.TryAttachProjectileVisual(
                root,
                embeddedResourceName,
                relativePathFromModDir,
                ThrownVisualScaleVector))
        {
            ProceduralVisualFactory.CreateEllipsoid(
                $"{objectName}FallbackVisual",
                root.transform,
                Vector3.zero,
                Quaternion.identity,
                ThrownVisualScaleVector,
                fallbackColor,
                root.layer);
        }

        return root;
    }
}
