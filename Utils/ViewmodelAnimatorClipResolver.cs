using System;
using System.Collections.Generic;
using UnityEngine;
using MoreWeapons.Weapons;

namespace MoreWeapons.Utils;

internal static class ViewmodelAnimatorClipResolver
{
    internal static AnimationClip ResolveSourceClip(PlaceholderAvatarWeaponEquippable weapon, string presetName) =>
        ResolveSourceClip(ViewmodelAvatarBoneHelper.Animator?.runtimeAnimatorController, weapon, presetName, null);

    internal static AnimationClip ResolveSourceClip(
        RuntimeAnimatorController controller,
        PlaceholderAvatarWeaponEquippable weapon,
        string presetName,
        string preferredClipName)
    {
        if (controller == null)
            return null;

        var clips = controller.animationClips;
        if (clips == null || clips.Length == 0)
            return null;

        if (!string.IsNullOrEmpty(preferredClipName))
        {
            foreach (var clip in clips)
            {
                if (clip != null && clip.name == preferredClipName)
                    return clip;
            }
        }

        foreach (var hint in GetSearchHints(presetName, weapon))
        {
            if (string.IsNullOrEmpty(hint))
                continue;

            foreach (var clip in clips)
            {
                if (clip == null)
                    continue;

                if (clip.name.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0)
                    return clip;
            }
        }

        return clips[0];
    }

    internal static AnimationClip CreateEditableCopy(AnimationClip source)
    {
        if (source == null)
            return null;

        var copy = UnityEngine.Object.Instantiate(source);
        copy.name = source.name + "_Edit";
        return copy;
    }

    private static IEnumerable<string> GetSearchHints(string presetName, PlaceholderAvatarWeaponEquippable weapon)
    {
        switch (presetName)
        {
            case "Equip":
                yield return weapon?.EquipTrigger;
                yield return "Equip";
                yield break;
            case "Fire":
                if (weapon?.EditorFireAnimTriggers != null)
                {
                    foreach (var trigger in weapon.EditorFireAnimTriggers)
                        yield return trigger;
                }
                yield return "Fire";
                yield return "Shoot";
                yield return "Recoil";
                yield break;
            default:
                if (presetName.StartsWith("Fire", StringComparison.OrdinalIgnoreCase))
                {
                    var index = 0;
                    if (presetName.Length > 4 && int.TryParse(presetName.Substring(4), out var parsed))
                        index = parsed - 1;

                    if (weapon?.EditorFireAnimTriggers != null && index >= 0 && index < weapon.EditorFireAnimTriggers.Length)
                        yield return weapon.EditorFireAnimTriggers[index];

                    yield return "Fire";
                    yield return "Shoot";
                    yield break;
                }

                if (presetName == "ReloadStart")
                {
                    yield return weapon?.EditorReloadStartAnimTrigger;
                    yield return "ReloadStart";
                    yield return "MagazineReload";
                    yield break;
                }

                if (presetName == "ReloadIndividual")
                {
                    yield return weapon?.EditorReloadIndividualAnimTrigger;
                    yield return "ReloadIndividual";
                    yield return "Insert";
                    yield return "Reload";
                    yield break;
                }

                if (presetName == "ReloadEnd")
                {
                    yield return weapon?.EditorReloadEndAnimTrigger;
                    yield return "ReloadEnd";
                    yield return "Reload";
                    yield break;
                }

                if (presetName == "Cock")
                {
                    yield return weapon?.EditorCockAnimTrigger;
                    yield return "Cock";
                    yield return "Pump";
                    yield break;
                }

                yield return presetName;
                yield break;
        }
    }
}
