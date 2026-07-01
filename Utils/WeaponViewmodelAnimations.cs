namespace MoreWeapons.Utils;

/// <summary>
/// Baked viewmodel animation keyframes keyed by <see cref="ViewmodelProfile.Label"/>.
/// Populate via F5 editor session data or paste exported C# snippets here.
/// </summary>
internal static class WeaponViewmodelAnimations
{
    internal static ViewmodelWeaponAnimSet TryGet(string profileLabel)
    {
        if (string.IsNullOrEmpty(profileLabel))
            return null;

        return _sets.TryGetValue(profileLabel, out var set) ? set : null;
    }

    private static readonly System.Collections.Generic.Dictionary<string, ViewmodelWeaponAnimSet> _sets =
        new();
}
