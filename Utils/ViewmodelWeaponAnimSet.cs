using System.Collections.Generic;

namespace MoreWeapons.Utils;

internal sealed class ViewmodelWeaponAnimSet
{
    private readonly Dictionary<string, ViewmodelAnimClip> _clips = new();

    internal bool HasAnyKeyframes
    {
        get
        {
            foreach (var clip in _clips.Values)
            {
                if (clip != null && clip.Keyframes.Count > 0)
                    return true;
            }

            return false;
        }
    }

    internal IEnumerable<KeyValuePair<string, ViewmodelAnimClip>> Clips => _clips;

    internal bool TryGetClip(string presetName, out ViewmodelAnimClip clip) =>
        _clips.TryGetValue(presetName, out clip);

    internal void SetClip(string presetName, ViewmodelAnimClip clip)
    {
        if (string.IsNullOrEmpty(presetName) || clip == null)
            return;

        _clips[presetName] = clip;
    }

    internal ViewmodelWeaponAnimSet Clone()
    {
        var clone = new ViewmodelWeaponAnimSet();
        foreach (var pair in _clips)
            clone.SetClip(pair.Key, pair.Value?.Clone());
        return clone;
    }

    internal static ViewmodelWeaponAnimSet FromLibrary(IEnumerable<KeyValuePair<string, ViewmodelAnimClip>> library)
    {
        var set = new ViewmodelWeaponAnimSet();
        if (library == null)
            return set;

        foreach (var pair in library)
        {
            if (pair.Value == null || pair.Value.Keyframes.Count == 0)
                continue;

            set.SetClip(pair.Key, pair.Value.Clone());
        }

        return set;
    }
}
