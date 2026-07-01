using System.Collections.Generic;
using MoreWeapons.Weapons;
using UnityEngine;

namespace MoreWeapons.Utils;

internal static class ViewmodelWeaponAnimatorFactory
{
    internal readonly struct OverrideBuildResult
    {
        internal OverrideBuildResult(RuntimeAnimatorController controller, List<UnityEngine.Object> assets)
        {
            Controller = controller;
            Assets = assets;
        }

        internal RuntimeAnimatorController Controller { get; }
        internal List<UnityEngine.Object> Assets { get; }
        internal bool Succeeded => Controller != null;
    }

    internal static OverrideBuildResult TryCreateOverride(
        PlaceholderAvatarWeaponEquippable weapon,
        RuntimeAnimatorController baseController,
        ViewmodelWeaponAnimSet animSet)
    {
        if (weapon == null || baseController == null || animSet == null || !animSet.HasAnyKeyframes)
            return default;

        ViewmodelAvatarBoneHelper.PrepareRuntimeSampling();
        var sampleRoot = ViewmodelAvatarBoneHelper.GetSampleRoot();
        if (sampleRoot == null)
            return default;

        var assets = new List<UnityEngine.Object>();
        var overrideController = new AnimatorOverrideController(baseController)
        {
            name = weapon.GetViewmodelProfile().Label + "_ViewmodelOverride",
        };
        assets.Add(overrideController);
        UnityEngine.Object.DontDestroyOnLoad(overrideController);

        var replacedAny = false;
        foreach (var pair in animSet.Clips)
        {
            var presetName = pair.Key;
            var animClip = pair.Value;
            if (animClip == null || animClip.Keyframes.Count == 0)
                continue;

            var sourceClip = ViewmodelAnimatorClipResolver.ResolveSourceClip(
                baseController,
                weapon,
                presetName,
                animClip.SourceClipName);
            if (sourceClip == null)
                continue;

            var bakedClip = ViewmodelAnimatorBuilder.BuildOverrideClip(sourceClip, animClip, sampleRoot);
            if (bakedClip == null)
                continue;

            bakedClip.name = sourceClip.name + "_MW";
            assets.Add(bakedClip);
            UnityEngine.Object.DontDestroyOnLoad(bakedClip);
            overrideController[sourceClip] = bakedClip;
            replacedAny = true;
        }

        if (!replacedAny)
        {
            foreach (var asset in assets)
                UnityEngine.Object.Destroy(asset);

            return default;
        }

        return new OverrideBuildResult(overrideController, assets);
    }
}
