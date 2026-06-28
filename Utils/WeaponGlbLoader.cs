using S1MAPI.Gltf;
using System.Collections.Generic;
#if IL2CPP
using Il2CppScheduleOne.DevUtilities;
#else
using ScheduleOne.DevUtilities;
#endif
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class WeaponGlbLoader
{
    private static readonly Dictionary<string, GameObject> ModelCache = new();

    internal static GameObject LoadViewmodel(string embeddedResourceName, string relativePathFromModDir) =>
        LoadGlb(embeddedResourceName, relativePathFromModDir, castShadows: false);

    internal static GameObject LoadThirdPersonModel(string embeddedResourceName, string relativePathFromModDir) =>
        LoadGlb(embeddedResourceName, relativePathFromModDir, castShadows: true);

    private static GameObject LoadGlb(string embeddedResourceName, string relativePathFromModDir, bool castShadows)
    {
        var cacheKey = $"{embeddedResourceName}|{relativePathFromModDir}|{castShadows}";
        if (ModelCache.TryGetValue(cacheKey, out var cached) && cached != null)
            return InstantiateCached(cached);

        var bytes = ModAssetLoader.LoadBytes(embeddedResourceName, relativePathFromModDir);
        if (bytes == null || bytes.Length == 0)
        {
            MelonLoader.MelonLogger.Warning($"MoreWeapons: missing GLB '{relativePathFromModDir}'.");
            return null;
        }

        var model = GltfLoader.LoadGlb(bytes);
        if (model == null)
        {
            MelonLoader.MelonLogger.Warning($"MoreWeapons: failed to load GLB '{relativePathFromModDir}'.");
            return null;
        }

        foreach (var collider in model.GetComponentsInChildren<Collider>(true))
            UnityEngine.Object.Destroy(collider);

        var shadowMode = castShadows
            ? UnityEngine.Rendering.ShadowCastingMode.On
            : UnityEngine.Rendering.ShadowCastingMode.Off;

        foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = shadowMode;
            renderer.enabled = true;
        }

        if (!castShadows)
            MeshRenderHelper.ConfigureDoubleSidedRenderers(model);

        model.name = $"Cached_{System.IO.Path.GetFileNameWithoutExtension(relativePathFromModDir)}";
        model.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(model);
        ModelCache[cacheKey] = model;

        return InstantiateCached(model);
    }

    private static GameObject InstantiateCached(GameObject cached)
    {
        var clone = UnityEngine.Object.Instantiate(cached);
        clone.SetActive(true);
        return clone;
    }

    internal static void AttachThirdPersonModel(
        GameObject model,
        Transform parent,
        Vector3 localPosition,
        Quaternion localRotation,
        float uniformWorldScale)
    {
        model.transform.SetParent(parent, false);
        model.transform.localPosition = localPosition;
        model.transform.localRotation = localRotation;

        var parentScale = parent.lossyScale;
        model.transform.localScale = new Vector3(
            uniformWorldScale / Mathf.Max(parentScale.x, 0.001f),
            uniformWorldScale / Mathf.Max(parentScale.y, 0.001f),
            uniformWorldScale / Mathf.Max(parentScale.z, 0.001f));

        LayerUtility.SetLayerRecursively(model, LayerMask.NameToLayer("Default"));
    }

    private const float ViewmodelYaw = 180f;
    private const float AimPitchDegrees = -9f;
    private const float AimYawDegrees = 18f;
    private const float AimForwardOffset = 0.05f;

    internal static Quaternion GetViewmodelAimRotation(float aim) =>
        Quaternion.Euler(aim * AimPitchDegrees, ViewmodelYaw + aim * AimYawDegrees, 0f);

    internal static Vector3 GetViewmodelAimPositionOffset(float aim) =>
        new Vector3(0f, 0f, aim * AimForwardOffset);

    internal static void ApplyViewmodelProfile(GameObject model, ViewmodelProfile profile)
    {
        model.transform.localPosition = profile.HipPosition;
        model.transform.localRotation = Quaternion.Euler(profile.HipEuler);
        model.transform.localScale = Vector3.one * profile.UniformScale;
        LayerUtility.SetLayerRecursively(model, LayerMask.NameToLayer("Viewmodel"));
    }

    internal static GameObject CreateProfiledViewmodel(
        string embeddedResourceName,
        string relativePathFromModDir,
        string objectName,
        ViewmodelProfile profile)
    {
        var model = LoadViewmodel(embeddedResourceName, relativePathFromModDir);
        if (model == null)
            return null;

        model.name = objectName;
        ApplyViewmodelProfile(model, profile);
        return model;
    }

    internal static void AttachProfiledThirdPersonModel(
        GameObject model,
        Transform parent,
        ViewmodelProfile profile)
    {
        AttachThirdPersonModel(
            model,
            parent,
            profile.ThirdPersonPosition,
            Quaternion.Euler(profile.ThirdPersonEuler),
            profile.ThirdPersonWorldScale);
    }

    internal static bool TryAttachProjectileVisual(
        GameObject root,
        string embeddedResourceName,
        string relativePathFromModDir,
        Vector3 localScale,
        Vector3 localEuler = default)
    {
        var model = LoadThirdPersonModel(embeddedResourceName, relativePathFromModDir);
        if (model == null)
            return false;

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            UnityEngine.Object.Destroy(renderer);

        model.transform.SetParent(root.transform, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(localEuler);
        model.transform.localScale = localScale;
        MeshRenderHelper.ConfigureDoubleSidedRenderers(model);
        return true;
    }
}
